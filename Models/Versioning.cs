using App1.Models.Ports;

namespace App1.Models
{
    public class Versioning : IVersionTool
    {
        public Versioning(ISaver Saver, IFileManager FileManager, ITransport Transport) 
        {
            fileManager = FileManager;
            transport = Transport;
            workspace = new MusicSyncWorkspace(Saver, FileManager);
        }

        public async Task uploadSongAsync(Song song, string title, string description, string versionNumber)
        {
            string songWorkspace = workspace.workspaceForSong(song);
            if (!transport.initiated(songWorkspace))
            {
                transport.init(songWorkspace, song.Guid.ToString());
            } 
            await compressSongAsync(song);
            await transport.uploadAllFilesAsync(songWorkspace, title, description);
            transport.tag(songWorkspace, versionNumber);
        }

        public async Task uploadFileForSongAsync(Song song, string file, string title)
        {
            string songWorkspace = workspace.workspaceForSong(song);
            fileManager.SyncFile(song.LocalPath, songWorkspace, file);
            await transport.uploadFileAsync(songWorkspace, file, title);
        }

        public async Task updateSongAsync(Song song)
        {
            string songWorkspace = workspace.workspaceForSong(song);
            await transport.downloadLastUpdateAsync(songWorkspace);
            await uncompressSongAsync(song);
        }

        public async Task<bool> updatesAvailableForSongAsync(Song song)
        {
            string songWorkspace = workspace.workspaceForSong(song);
            return await transport.updatesAvailbleAsync(songWorkspace);
        }

        public async Task revertSongAsync(Song song)
        {
            string songWorkspace = workspace.workspaceForSong(song);
            await transport.revertToLastLocalVersionAsync(songWorkspace);
            await uncompressSongAsync(song);
        }

        public async Task<SongVersion> currentVersionAsync(Song song)
        {
            return await Task.Run(() =>
            {
                string songWorkspace = workspace.workspaceForSong(song);
                return transport.lastLocalVersionAsync(songWorkspace);
            });       
        }

        public async Task<List<SongVersion>> versionsAsync(Song song)
        {
            return await Task.Run(() =>
            {
                string songWorkspace = workspace.workspaceForSong(song);
                return transport.localVersionsAsync(songWorkspace);
            });
        }

        public async Task<List<SongVersion>> upcomingVersionsAsync(Song song)
        {
            return await Task.Run(() =>
            {
                string songWorkspace = workspace.workspaceForSong(song);
                return transport.upcomingVersionsAsync(songWorkspace);
            });
        }

        public async Task downloadSharedSongAsync(string sharedLink, string songPath)
        {
            string songWorkspace = workspace.musicSyncPathFromSharedLink(sharedLink);
            await transport.initAsync(songWorkspace, sharedLink);
            songPath = fileManager.FormatPath(songPath);
            await uncompressSongAsync(songWorkspace, songPath);
        }

        public string shareSong(Song song)
        {
            string songWorkspace = workspace.workspaceForSong(song);
            return transport.shareLink(songWorkspace);
        }

        public async Task<string> newVersionNumberAsync(Song song, bool compo, bool mix, bool mastering)
        {
            SongVersion currentVersion = await currentVersionAsync(song);

            string versionNumber = currentVersion.Number;
            int compoNumber = 0;
            int mixNumber = 0;
            int masteringNumber = 0;
            if (versionNumber != string.Empty)
            {
                var numbers = versionNumber.Split('.').Select(int.Parse).ToList();
                compoNumber = numbers[0];
                mixNumber = numbers[1];
                masteringNumber = numbers[2];
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
            }
            else
            {
                if (compo)
                {
                    compoNumber++;
                }
                if (mix)
                {
                    mixNumber++;
                }
                if (mastering)
                {
                    masteringNumber++;
                }
            }
            versionNumber = compoNumber + "." + mixNumber + "." + masteringNumber;
            return versionNumber;
        }

        private async Task compressSongAsync(Song song)
        {
            string songWorkspace = workspace.workspaceForSong(song);
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
            string tmpDirectory = workspace.musicSyncFolder + @"tmpDirectory\";
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
            string songWorkspace = workspace.workspaceForSong(song);
            await uncompressSongAsync(songWorkspace, song.LocalPath);
        }

        private async Task uncompressSongAsync(string repoPath, string songPath)
        {
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
        private readonly MusicSyncWorkspace workspace;
    }
}
