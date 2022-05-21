using App1.Models.Ports;

namespace App1.Models
{
    public class Versioning
    {
        public Versioning(ISaver Saver, IFileManager FileManager, ITransport Transport) 
        {
            fileManager = FileManager;
            transport = Transport;
            musicSyncWorkspace = new MusicSyncWorkspace(Saver, FileManager);
        }

        public async Task uploadSongAsync(Song song, string title, string description, string versionNumber)
        {
            string songWorkspace = musicSyncWorkspace.GetWorkspaceForSong(song);
            if (!transport.Initiated(songWorkspace))
            {
                transport.Init(songWorkspace, song.Guid.ToString());
            } 
            await compressSongAsync(song);
            await transport.UploadAllFilesAsync(songWorkspace, title, description);
            transport.Tag(songWorkspace, versionNumber);
        }

        public async Task uploadFileForSongAsync(Song song, string file, string title)
        {
            string songWorkspace = musicSyncWorkspace.GetWorkspaceForSong(song);
            fileManager.SyncFile(song.LocalPath, songWorkspace, file);
            await transport.UploadFileAsync(songWorkspace, file, title);
        }

        public async Task updateSongAsync(Song song)
        {
            string songWorkspace = musicSyncWorkspace.GetWorkspaceForSong(song);
            await transport.DownloadLastUpdateAsync(songWorkspace);
            await uncompressSongAsync(song);
        }

        public async Task<bool> updatesAvailableForSongAsync(Song song)
        {
            string songWorkspace = musicSyncWorkspace.GetWorkspaceForSong(song);
            return await transport.UpdatesAvailbleAsync(songWorkspace);
        }

        public async Task revertSongAsync(Song song)
        {
            string songWorkspace = musicSyncWorkspace.GetWorkspaceForSong(song);
            await transport.RevertToLastLocalVersionAsync(songWorkspace);
            await uncompressSongAsync(song);
        }

        public async Task<SongVersion> currentVersionAsync(Song song)
        {
            return await Task.Run(() =>
            {
                string songWorkspace = musicSyncWorkspace.GetWorkspaceForSong(song);
                return transport.LastLocalVersionAsync(songWorkspace);
            });       
        }

        public async Task<List<SongVersion>> versionsAsync(Song song)
        {
            return await Task.Run(() =>
            {
                string songWorkspace = musicSyncWorkspace.GetWorkspaceForSong(song);
                return transport.LocalVersionsAsync(songWorkspace);
            });
        }

        public async Task<List<SongVersion>> upcomingVersionsAsync(Song song)
        {
            return await Task.Run(() =>
            {
                string songWorkspace = musicSyncWorkspace.GetWorkspaceForSong(song);
                return transport.UpcomingVersionsAsync(songWorkspace);
            });
        }

        public async Task downloadSharedSongAsync(string sharedLink, string songPath)
        {
            var songGuid = transport.GuidFromSharedLink(sharedLink);
            string songWorkspace = musicSyncWorkspace.GetWorkspace(songGuid);
            await transport.InitAsync(songWorkspace, sharedLink);
            await uncompressSongAsync(songWorkspace, songPath);
        }

        public string GuidFromSharedLink(string sharedLink)
        {
            return transport.GuidFromSharedLink(sharedLink);
        }

        public string shareSong(Song song)
        {
            string songWorkspace = musicSyncWorkspace.GetWorkspaceForSong(song);
            return transport.ShareLink(songWorkspace);
        }

        public async Task<string> newVersionNumberAsync(Song song, bool compo, bool mix, bool mastering)
        {
            SongVersion currentVersion = await currentVersionAsync(song);

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

        private async Task compressSongAsync(Song song)
        {
            string songWorkspace = musicSyncWorkspace.GetWorkspaceForSong(song);
            string? songArchive = await fileManager.findFileNameBasedOnExtensionAsync(songWorkspace, ".zip");
            if (songArchive != null)
            {
                fileManager.DeleteFile(songArchive, songWorkspace);
            }
            string pathToSongWithSelectedFodlers = await selectFoldersToBeCompressed(song);
            await fileManager.CompressDirectoryAsync(pathToSongWithSelectedFodlers, song.Guid + ".zip", songWorkspace);
            fileManager.DeleteDirectory(pathToSongWithSelectedFodlers);
        }

        private async Task<string> selectFoldersToBeCompressed(Song song)
        {
            string tmpDirectory = musicSyncWorkspace.musicSyncFolder + @"tmpDirectory\";
            if (fileManager.DirectoryExists(tmpDirectory))
            {
                fileManager.DeleteDirectory(tmpDirectory);
            }
            fileManager.CreateDirectory(ref tmpDirectory);

            string? songFile = await fileManager.findFileNameBasedOnExtensionAsync(song.LocalPath, ".song");
            if (songFile != null)
            {
                await fileManager.CopyFileAsync(songFile, song.LocalPath, tmpDirectory);
            }
            
            List<string> foldersToBeCopied = new List<string>();
            string mediaFolder = "Media";
            string melodyneFolder = "Melodyne";
            foldersToBeCopied.Add(mediaFolder);
            foldersToBeCopied.Add(melodyneFolder);

            fileManager.CopyDirectories(foldersToBeCopied,song.LocalPath,tmpDirectory);

            return tmpDirectory;
        }

        private async Task uncompressSongAsync(Song song)
        {
            string songWorkspace = musicSyncWorkspace.GetWorkspaceForSong(song);
            await uncompressSongAsync(songWorkspace, song.LocalPath);
        }

        private async Task uncompressSongAsync(string repoPath, string songPath)
        {
            fileManager.FormatPath(songPath);
            string? zipFile = await fileManager.findFileNameBasedOnExtensionAsync(repoPath, ".zip");
            if (zipFile != null)
            {
                string? songFile = await fileManager.findFileNameBasedOnExtensionAsync(songPath, ".song");
                if (songFile != null)
                {
                    fileManager.DeleteFile(songFile, songPath);
                }
                await fileManager.UncompressArchiveAsync(repoPath + zipFile, songPath);
            }
            fileManager.SyncFile(repoPath, songPath, ".lock");
        }

        private readonly IFileManager fileManager;
        private readonly ITransport transport;
        private readonly MusicSyncWorkspace musicSyncWorkspace;
    }
}
