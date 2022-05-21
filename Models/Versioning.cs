using App1.Models.Ports;

namespace App1.Models
{
    public class Versioning
    {
        public Versioning(ISaver saver, IFileManager fileManager, ITransport transport) 
        {
            _fileManager = fileManager;
            _transport = transport;
            _musicSyncWorkspace = new MusicSyncWorkspace(saver, fileManager);
        }

        public async Task UploadSongAsync(Song song, string title, string description, string versionNumber)
        {
            string songWorkspace = _musicSyncWorkspace.GetWorkspaceForSong(song);
            if (!_transport.Initiated(songWorkspace))
            {
                _transport.Init(songWorkspace, song.Guid.ToString());
            } 
            await CompressSongAsync(song);
            await _transport.UploadAllFilesAsync(songWorkspace, title, description);
            _transport.Tag(songWorkspace, versionNumber);
        }

        public async Task UploadFileForSongAsync(Song song, string file, string title)
        {
            string songWorkspace = _musicSyncWorkspace.GetWorkspaceForSong(song);
            _fileManager.SyncFile(song.LocalPath, songWorkspace, file);
            await _transport.UploadFileAsync(songWorkspace, file, title);
        }

        public async Task UpdateSongAsync(Song song)
        {
            string songWorkspace = _musicSyncWorkspace.GetWorkspaceForSong(song);
            await _transport.DownloadLastUpdateAsync(songWorkspace);
            await UncompressSongAsync(song);
        }

        public async Task<bool> UpdatesAvailableForSongAsync(Song song)
        {
            string songWorkspace = _musicSyncWorkspace.GetWorkspaceForSong(song);
            return await _transport.UpdatesAvailbleAsync(songWorkspace);
        }

        public async Task RevertSongAsync(Song song)
        {
            string songWorkspace = _musicSyncWorkspace.GetWorkspaceForSong(song);
            await _transport.RevertToLastLocalVersionAsync(songWorkspace);
            await UncompressSongAsync(song);
        }

        public async Task<SongVersion> CurrentVersionAsync(Song song)
        {
            return await Task.Run(() =>
            {
                string songWorkspace = _musicSyncWorkspace.GetWorkspaceForSong(song);
                return _transport.LastLocalVersionAsync(songWorkspace);
            });       
        }

        public async Task<List<SongVersion>> VersionsAsync(Song song)
        {
            return await Task.Run(() =>
            {
                string songWorkspace = _musicSyncWorkspace.GetWorkspaceForSong(song);
                return _transport.LocalVersionsAsync(songWorkspace);
            });
        }

        public async Task<List<SongVersion>> UpcomingVersionsAsync(Song song)
        {
            return await Task.Run(() =>
            {
                string songWorkspace = _musicSyncWorkspace.GetWorkspaceForSong(song);
                return _transport.UpcomingVersionsAsync(songWorkspace);
            });
        }

        public async Task DownloadSharedSongAsync(string sharedLink, string songPath)
        {
            var songGuid = _transport.GuidFromSharedLink(sharedLink);
            string songWorkspace = _musicSyncWorkspace.GetWorkspace(songGuid);
            await _transport.InitAsync(songWorkspace, sharedLink);
            await UncompressSongAsync(songWorkspace, songPath);
        }

        public string GuidFromSharedLink(string sharedLink)
        {
            return _transport.GuidFromSharedLink(sharedLink);
        }

        public string ShareSong(Song song)
        {
            string songWorkspace = _musicSyncWorkspace.GetWorkspaceForSong(song);
            return _transport.ShareLink(songWorkspace);
        }

        public async Task<string> NewVersionNumberAsync(Song song, bool compo, bool mix, bool mastering)
        {
            SongVersion currentVersion = await CurrentVersionAsync(song);

            var numbers = currentVersion.Number.Split('.').Select(int.Parse).ToList();
            int compoNumber = numbers[0];
            int mixNumber = numbers[1];
            int masteringNumber = numbers[2];
            if (compo)
            {
                compoNumber++;
                mixNumber = 0;
                masteringNumber = 0;
            }
            if (mix)
            {
                mixNumber++;
                masteringNumber = 0;
            }
            if (mastering)
            {
                masteringNumber++;
            }
            string versionNumber = compoNumber + "." + mixNumber + "." + masteringNumber;
            return versionNumber;
        }

        private async Task CompressSongAsync(Song song)
        {
            string songWorkspace = _musicSyncWorkspace.GetWorkspaceForSong(song);
            string? songArchive = await _fileManager.FindFileNameBasedOnExtensionAsync(songWorkspace, ".zip");
            if (songArchive != null)
            {
                _fileManager.DeleteFile(songArchive, songWorkspace);
            }
            string pathToSongWithSelectedFodlers = await SelectFoldersToBeCompressed(song);
            await _fileManager.CompressDirectoryAsync(pathToSongWithSelectedFodlers, song.Guid + ".zip", songWorkspace);
            _fileManager.DeleteDirectory(pathToSongWithSelectedFodlers);
        }

        private async Task<string> SelectFoldersToBeCompressed(Song song)
        {
            string tmpDirectory = _musicSyncWorkspace.MusicSyncFolder + @"tmpDirectory\";
            if (_fileManager.DirectoryExists(tmpDirectory))
            {
                _fileManager.DeleteDirectory(tmpDirectory);
            }
            _fileManager.CreateDirectory(ref tmpDirectory);

            string? songFile = await _fileManager.FindFileNameBasedOnExtensionAsync(song.LocalPath, ".song");
            if (songFile != null)
            {
                await _fileManager.CopyFileAsync(songFile, song.LocalPath, tmpDirectory);
            }

            string mediaFolder = "Media";
            string melodyneFolder = "Melodyne";
            var foldersToBeCopied = new List<string>() { mediaFolder, melodyneFolder };

            _fileManager.CopyDirectories(foldersToBeCopied,song.LocalPath,tmpDirectory);

            return tmpDirectory;
        }

        private async Task UncompressSongAsync(Song song)
        {
            string songWorkspace = _musicSyncWorkspace.GetWorkspaceForSong(song);
            await UncompressSongAsync(songWorkspace, song.LocalPath);
        }

        private async Task UncompressSongAsync(string repoPath, string songPath)
        {
            _fileManager.FormatPath(songPath);
            string? zipFile = await _fileManager.FindFileNameBasedOnExtensionAsync(repoPath, ".zip");
            if (zipFile != null)
            {
                string? songFile = await _fileManager.FindFileNameBasedOnExtensionAsync(songPath, ".song");
                if (songFile != null)
                {
                    _fileManager.DeleteFile(songFile, songPath);
                }
                await _fileManager.UncompressArchiveAsync(repoPath + zipFile, songPath);
            }
            _fileManager.SyncFile(repoPath, songPath, ".lock");
        }

        private readonly IFileManager _fileManager;
        private readonly ITransport _transport;
        private readonly MusicSyncWorkspace _musicSyncWorkspace;
    }
}
