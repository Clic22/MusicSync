using App1.Models;
using App1.Models.Ports;
using LibGit2Sharp;
using System.IO.Compression;

namespace GitVersionTool
{
    public class GitSongVersioning : IVersionTool
    {
        public GitSongVersioning(string askedMusiSyncFolderLocation, ISaver newSaver, IFileManager newFileManager) 
        {
            musicSyncFolder = string.Empty;
            saver = newSaver;
            fileManager = newFileManager;
            createMusicSyncFolder(askedMusiSyncFolderLocation);
            git = new Git(newSaver, newFileManager);
        }

        public async Task<string> uploadSongAsync(Song song, string title, string description, string versionNumber)
        {
            try
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
                return String.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> uploadSongAsync(Song song, string file, string title)
        {
            try
            {
                await Task.Run(() =>
                {
                    string songMusicSyncPath = getMusicSyncPathForSong(song);
                    fileManager.SyncFile(song.LocalPath, songMusicSyncPath, file);
                    git.add(songMusicSyncPath, file);
                    git.commit(songMusicSyncPath, title, string.Empty);
                    git.push(songMusicSyncPath);
                });
                return String.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> updateSongAsync(Song song)
        {
            string errorMessage = await updateSongFromRepoAsync(song);
            if (errorMessage != String.Empty)
            {
                return errorMessage;
            }
            await uncompressSongAsync(song);
            return String.Empty;
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

        public async Task<string> revertSongAsync(Song song)
        {
            string errorMessage = await revertSongFromRepoAsync(song);
            if (errorMessage != String.Empty)
            {
                return errorMessage;
            }
            await uncompressSongAsync(song);
            return String.Empty;
        }

        public async Task<SongVersion> currentVersionAsync(Song song)
        {
            return await Task.Run(() =>
            {
                SongVersion currentVersion = new SongVersion();
                try
                {
                    string songMusicSyncPath = getMusicSyncPathForSong(song);
                    Tag lastTag = git.lastTag(songMusicSyncPath);
                    currentVersion.Number = lastTag.FriendlyName;
                    Commit commitTagged = (Commit)lastTag.Target;
                    currentVersion.Description = commitTagged.Message.Remove(commitTagged.Message.Length - 1);
                    currentVersion.Author = commitTagged.Author.Name;
                    return currentVersion;
                }
                catch
                {
                    return currentVersion;
                }
            });       
        }

        public async Task<List<SongVersion>> versionsAsync(Song song)
        {
            return await Task.Run(() =>
            {
                string songMusicSyncPath = getMusicSyncPathForSong(song);
                List<SongVersion> versions = new List<SongVersion>();
                TagCollection Tags = git.tags(songMusicSyncPath);
                foreach (var tag in Tags)
                {
                    SongVersion version = new SongVersion();
                    version.Number = tag.FriendlyName;
                    Commit commitTagged = (Commit)tag.Target;
                    version.Description = commitTagged.Message.Remove(commitTagged.Message.Length - 1);
                    version.Author = commitTagged.Author.Name;
                    versions.Add(version);
                }
                return versions;
            });
        }

        public async Task<string> downloadSharedSongAsync(string songFolder, string sharedLink, string downloadLocalPath)
        {
            string songMusicSyncPath = getMusicSyncPathForFolder(songFolder);
            string errorMessage = await downloadSharedSongFromRepoAsync(sharedLink, songMusicSyncPath);
            if (errorMessage != String.Empty)
            {
                return errorMessage;
            }
            downloadLocalPath = fileManager.FormatPath(downloadLocalPath);
            await uncompressSongAsync(songFolder, downloadLocalPath, songMusicSyncPath);
            return String.Empty;
        }

        public async Task<string> shareSongAsync(Song song)
        {
            try
            {
                return await Task.Run(() =>
                {
                    string songMusicSyncPath = getMusicSyncPathForSong(song);
                    return git.remoteUrl(songMusicSyncPath);
                });

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private async Task<string> updateSongFromRepoAsync(Song song)
        {
            try
            {
                await Task.Run(() =>
                {
                    string songMusicSyncPath = getMusicSyncPathForSong(song);
                    git.pull(songMusicSyncPath);
                });
                return String.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private async Task<string> downloadSharedSongFromRepoAsync(string sharedLink, string downloadPath)
        {
            try
            {
                await Task.Run(() =>
                {
                    git.clone(sharedLink, downloadPath);
                });
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private async Task<string> revertSongFromRepoAsync(Song song)
        {
            try
            {
                await Task.Run(() =>
                {
                    string songMusicSyncPath = getMusicSyncPathForSong(song);
                    git.resetMasterHard(songMusicSyncPath);
                });
                return String.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
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
        private readonly ISaver saver;
        private readonly IFileManager fileManager;
        private readonly Git git;
    }
}
