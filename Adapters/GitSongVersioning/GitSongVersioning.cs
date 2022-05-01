using App1.Models;
using App1.Models.Ports;
using LibGit2Sharp;
using System.IO.Compression;

namespace GitVersionTool
{
    public class GitSongVersioning : IVersionTool
    {
        public GitSongVersioning(ISaver newSaver, IFileManager newFileManager) 
        {
            musicSyncFolder = string.Empty;
            fileManager = newFileManager;
            createMusicSyncFolder(newSaver.savedMusicSyncFolder());
            git = new Git(newSaver, newFileManager);
        }

        public async Task uploadSongAsync(Song song, string title, string description, string versionNumber)
        {
            string songMusicSyncPath = getMusicSyncPathForSong(song);
            if (!git.initiated(songMusicSyncPath))
            {
                git.init(songMusicSyncPath, song.Title);
            }
            await compressSongAsync(song);
            await Task.Run(() =>
            {
                git.addAll(songMusicSyncPath);
                git.commit(songMusicSyncPath, title, description);
                git.push(songMusicSyncPath);
                git.tag(songMusicSyncPath, versionNumber);
            });

        }

        public async Task uploadSongAsync(Song song, string file, string title)
        {
            await Task.Run(() =>
            {
                string songMusicSyncPath = getMusicSyncPathForSong(song);
                fileManager.SyncFile(song.LocalPath, songMusicSyncPath, file);
                git.add(songMusicSyncPath, file);
                git.commit(songMusicSyncPath, title, string.Empty);
                git.push(songMusicSyncPath);
            });
        }

        public async Task updateSongAsync(Song song)
        {
            await updateSongFromRepoAsync(song);
            await uncompressSongAsync(song);
        }

        public async Task<bool> updatesAvailableForSongAsync(Song song)
        {
            return await Task.Run(() =>
            {
                string songMusicSyncPath = getMusicSyncPathForSong(song);
                int? behind = git.masterBranchIsBehindBy(songMusicSyncPath);
                if (behind != null)
                {
                    if (behind != 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            });
        }

        public async Task revertSongAsync(Song song)
        {
            await revertSongFromRepoAsync(song);
            await uncompressSongAsync(song);
        }

        public async Task<SongVersion> currentVersionAsync(Song song)
        {
            return await Task.Run(() =>
            {
                SongVersion currentVersion = new SongVersion();
                string songMusicSyncPath = getMusicSyncPathForSong(song);
                GitTag lastTag = git.lastLocalTag(songMusicSyncPath);
                currentVersion.Number = lastTag.Name;
                currentVersion.Description = lastTag.Description.Remove(lastTag.Description.Length - 1);
                currentVersion.Author = lastTag.Author;
                return currentVersion;
            });       
        }

        public async Task<List<SongVersion>> versionsAsync(Song song)
        {
            return await Task.Run(() =>
            {
                string songMusicSyncPath = getMusicSyncPathForSong(song);
                List<SongVersion> versions = new List<SongVersion>();
                var Tags = git.localTags(songMusicSyncPath);
                foreach (var tag in Tags)
                {
                    SongVersion version = new SongVersion();
                    version.Number = tag.Name;
                    version.Description = tag.Description.Remove(tag.Description.Length - 1);
                    version.Author = tag.Author;
                    versions.Add(version);
                }
                return versions;
            });
        }


        public async Task<List<SongVersion>> upcomingVersionsAsync(Song song)
        {
            return await Task.Run(() =>
            {
                string songMusicSyncPath = getMusicSyncPathForSong(song);
                List<SongVersion> upcomingVersions = new List<SongVersion>();
                var Tags = git.remoteTags(songMusicSyncPath);
                foreach (var tag in Tags)
                {
                    SongVersion version = new SongVersion();
                    version.Number = tag.Name;
                    version.Description = tag.Description.Remove(tag.Description.Length - 1);
                    version.Author = tag.Author;
                    upcomingVersions.Add(version);
                }
                return upcomingVersions;
            });
        }


        public async Task downloadSharedSongAsync(string songFolder, string sharedLink, string downloadLocalPath)
        {
            string songMusicSyncPath = getMusicSyncPathForFolder(songFolder);
            await downloadSharedSongFromRepoAsync(sharedLink, songMusicSyncPath);
            downloadLocalPath = fileManager.FormatPath(downloadLocalPath);
            await uncompressSongAsync(songFolder, downloadLocalPath, songMusicSyncPath);
        }

        public async Task<string> shareSongAsync(Song song)
        {
            return await Task.Run(() =>
            {
                string songMusicSyncPath = getMusicSyncPathForSong(song);
                return git.remoteUrl(songMusicSyncPath);
            });
        }

        private async Task updateSongFromRepoAsync(Song song)
        {
            await Task.Run(() =>
            {
                string songMusicSyncPath = getMusicSyncPathForSong(song);
                git.pull(songMusicSyncPath);
            });
        }

        private async Task downloadSharedSongFromRepoAsync(string sharedLink, string downloadPath)
        {
            await Task.Run(() =>
            {
                git.clone(sharedLink, downloadPath);
            });
        }

        private async Task revertSongFromRepoAsync(Song song)
        {
            await Task.Run(() =>
            {
                string songMusicSyncPath = getMusicSyncPathForSong(song);
                git.resetMasterHard(songMusicSyncPath);
            });

        }

        private async Task compressSongAsync(Song song)
        {
            string musicSyncFolderForSong = getMusicSyncPathForSong(song);
            string songArchive = song.Title + ".zip";
            if (fileManager.FileExists(songArchive, musicSyncFolderForSong))
            {
                fileManager.DeleteFile(songArchive, musicSyncFolderForSong);
            }
            string pathToSongWithSelectedFodlers = await selectFoldersToBeCompressed(song);
            await fileManager.CompressDirectoryAsync(pathToSongWithSelectedFodlers, song.Title + ".zip", getMusicSyncPathForSong(song));
            fileManager.DeleteDirectory(pathToSongWithSelectedFodlers);
        }

        private async Task<string> selectFoldersToBeCompressed(Song song)
        {
            string tmpDirectory = musicSyncFolder + @"tmpDirectory\";
            if (fileManager.DirectoryExists(tmpDirectory))
            {
                fileManager.DeleteDirectory(tmpDirectory);
            }
            fileManager.CreateDirectory(ref tmpDirectory);

            string songFile = await fileManager.findFileNameBasedOnExtensionAsync(song.LocalPath, ".song");
            await fileManager.CopyFileAsync(songFile, song.LocalPath, tmpDirectory);

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
            string repoPath = getMusicSyncPathForSong(song);
            string zipFile = await fileManager.findFileNameBasedOnExtensionAsync(repoPath, ".zip");
            await fileManager.UncompressArchiveAsync(repoPath + zipFile, song.LocalPath);
            fileManager.SyncFile(repoPath, song.LocalPath, ".lock");
        }

        private async Task uncompressSongAsync(string songFolder, string downloadLocalPath, string repoPath)
        {
            string zipFile = await fileManager.findFileNameBasedOnExtensionAsync(repoPath, ".zip");
            await fileManager.UncompressArchiveAsync(repoPath + zipFile, downloadLocalPath + songFolder );
            fileManager.SyncFile(repoPath, downloadLocalPath + songFolder, ".lock");
        }

        private string getMusicSyncPathForSong(Song song)
        {
            return musicSyncFolder + song.Title + @"\";
        }

        private string getMusicSyncPathForFolder(string songFolder)
        {
            return musicSyncFolder + songFolder;
        }

        private void createMusicSyncFolder(string askedMusicSyncFolderLocation)
        {
            if (!string.IsNullOrEmpty(askedMusicSyncFolderLocation))
            {
                musicSyncFolder = askedMusicSyncFolderLocation + @".musicsync\";
                fileManager.CreateDirectory(ref musicSyncFolder);
            }
        }

        private string musicSyncFolder;
        private readonly IFileManager fileManager;
        private readonly Git git;
    }
}
