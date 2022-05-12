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
            fileManager = newFileManager;
            saver = newSaver;
            git = new Git(newSaver, newFileManager);
        }

        public async Task uploadSongAsync(Song song, string title, string description, string versionNumber)
        {
            string songMusicSyncPath = getMusicSyncPathForSong(song);
            if (!git.initiated(songMusicSyncPath))
            {
                git.init(songMusicSyncPath, song.Guid.ToString());
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

        public async Task uploadFileForSongAsync(Song song, string file, string title)
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
                fillSongVersionFromTag(currentVersion, lastTag);
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
                fillSongVersionsFromTags(versions, Tags);
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
                fillSongVersionsFromTags(upcomingVersions, Tags);
                return upcomingVersions;
            });
        }

        public async Task downloadSharedSongAsync(string sharedLink, string songPath)
        {
            string musicSyncPath = getMusicSyncFolder();
            string songMusicSyncPath = await downloadSharedSongFromRepoAsync(sharedLink, musicSyncPath);
            songPath = fileManager.FormatPath(songPath);
            await uncompressSongAsync(songMusicSyncPath,songPath);
        }

        public async Task<string> shareSongAsync(Song song)
        {
            return await Task.Run(() =>
            {
                string songMusicSyncPath = getMusicSyncPathForSong(song);
                return git.remoteUrl(songMusicSyncPath);
            });
        }

        public string guidFromSharedLink(string sharedLink)
        {
            User user = saver.savedUser();
            string UrlStart = "https://gitlab.com/" + user.BandName.Replace(" ", "-") + "/";
            string UrlEnd = ".git";
            int startPos = sharedLink.LastIndexOf(UrlStart) + UrlStart.Length;
            int length = sharedLink.IndexOf(UrlEnd) - startPos;
            string sub = sharedLink.Substring(startPos, length);
            return sub;
        }

        private static void fillSongVersionsFromTags(List<SongVersion> versions, List<GitTag> Tags)
        {
            foreach (var tag in Tags)
            {
                SongVersion version = new SongVersion();
                fillSongVersionFromTag(version, tag);
                versions.Add(version);
            }
        }

        private static void fillSongVersionFromTag(SongVersion version, GitTag lastTag)
        {
            version.Number = lastTag.Name;
            version.Description = lastTag.Description.Remove(lastTag.Description.Length - 1);
            version.Author = lastTag.Author;
            version.Date = lastTag.Date;
        }

        private async Task updateSongFromRepoAsync(Song song)
        {
            await Task.Run(() =>
            {
                string songMusicSyncPath = getMusicSyncPathForSong(song);
                git.pull(songMusicSyncPath);
            });
        }

        private async Task<string> downloadSharedSongFromRepoAsync(string sharedLink, string musicSyncPath)
        {
            return await Task.Run(() =>
            {
                string musicSyncSongPath = getMusicSyncPathFromSharedLink(sharedLink);
                git.clone(sharedLink, musicSyncSongPath);
                return musicSyncSongPath;
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
            var musicSyncFolder = saver.savedMusicSyncFolder();
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

        private async Task uncompressSongAsync(string repoPath, string songPath)
        {
            string zipFile = await fileManager.findFileNameBasedOnExtensionAsync(repoPath, ".zip");
            await fileManager.UncompressArchiveAsync(repoPath + zipFile, songPath );
            fileManager.SyncFile(repoPath, songPath, ".lock");
        }

        private string getMusicSyncPathForSong(Song song)
        {
            string musicSyncFolder = getMusicSyncFolder();
            return fileManager.FormatPath(musicSyncFolder + song.Guid);
        }

        private string getMusicSyncPathFromSharedLink(string sharedLink)
        {
            string guid = guidFromSharedLink(sharedLink);
            string musicSyncFolder = getMusicSyncFolder();
            return fileManager.FormatPath(musicSyncFolder + guid) ;
        }

        private string getMusicSyncFolder()
        {
            var musicSyncFolder = saver.savedMusicSyncFolder() + @".musicsync\";
            fileManager.CreateDirectory(ref musicSyncFolder);
            return musicSyncFolder;
        }

        private readonly ISaver saver;
        private readonly IFileManager fileManager;
        private readonly Git git;
    }
}
