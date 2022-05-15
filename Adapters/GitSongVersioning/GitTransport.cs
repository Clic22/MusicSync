using App1.Models;
using App1.Models.Ports;
using LibGit2Sharp;
using System.IO.Compression;

namespace GitVersionTool
{
    public class GitTransport : ITransport
    {
        public GitTransport(ISaver Saver, IFileManager FileManager) 
        {
            saver = Saver;
            git = new Git(Saver, FileManager);
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

        public void init(string path, string name)
        {
            git.init(path, name);
        }

        public async Task initAsync(string musicSyncSongPath, string sharedLink)
        {
            await Task.Run(() =>
            {
                git.clone(sharedLink, musicSyncSongPath);
            });
        }

        public bool initiated(string path)
        {
            return git.initiated(path);
        }

        public async Task uploadAllFilesAsync(string songMusicSyncPath, string title, string description)
        {
            await Task.Run(() =>
            {
                git.addAllFiles(songMusicSyncPath);
                git.commit(songMusicSyncPath, title, description);
                git.push(songMusicSyncPath);
            });
        }

        public async Task uploadFileAsync(string songMusicSyncPath, string file, string title)
        {
            await Task.Run(() =>
            {
                git.add(songMusicSyncPath, file);
                git.commit(songMusicSyncPath, title, string.Empty);
                git.push(songMusicSyncPath);
            });
        }

        public void tag(string songMusicSyncPath, string versionNumber)
        {
            git.tag(songMusicSyncPath, versionNumber);
        }

        public async Task<bool> updatesAvailbleAsync(string songMusicSyncPath)
        {
            return await Task.Run(() =>
            {
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

        public async Task downloadLastUpdateAsync(string songMusicSyncPath)
        {
            await Task.Run(() =>
            {
                git.pull(songMusicSyncPath);
            });
        }

        public async Task revertToLastLocalVersionAsync(string songMusicSyncPath)
        {
            await Task.Run(() =>
            {
                git.resetMasterHard(songMusicSyncPath);
            });
        }

        public async Task<SongVersion>? lastLocalVersionAsync(string songMusicSyncPath)
        {
            return await Task.Run(() =>
            {
                SongVersion currentVersion = new SongVersion();
                GitTag lastTag = git.lastLocalTag(songMusicSyncPath);
                fillSongVersionFromTag(currentVersion, lastTag);
                return currentVersion;
            });
        }

        public async Task<List<SongVersion>>? localVersionsAsync(string songMusicSyncPath)
        {
            return await Task.Run(() =>
            {
                List<SongVersion> versions = new List<SongVersion>();
                var Tags = git.localTags(songMusicSyncPath);
                fillSongVersionsFromTags(versions, Tags);
                return versions;
            });
        }

        public async Task<List<SongVersion>>? upcomingVersionsAsync(string songMusicSyncPath)
        {
            return await Task.Run(() =>
            {
                List<SongVersion> upcomingVersions = new List<SongVersion>();
                var Tags = git.remoteTags(songMusicSyncPath);
                fillSongVersionsFromTags(upcomingVersions, Tags);
                return upcomingVersions;
            });
        }

        public string shareLink(string songMusicSyncPath)
        {
            return git.remoteUrl(songMusicSyncPath);
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

        private readonly ISaver saver;
        private readonly Git git;
    }
}
