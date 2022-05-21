using App1.Models;
using App1.Models.Ports;

namespace GitVersionTool
{
    public class GitTransport : ITransport
    {
        public GitTransport(string GitServerUrl, ISaver Saver, IFileManager FileManager) 
        {
            gitServerUrl = GitServerUrl;
            saver = Saver;
            git = new Git(Saver, FileManager);
        }

        public void Init(string songMusicSyncPath, string name)
        {
            git.init(songMusicSyncPath, name);
        }

        public async Task InitAsync(string songMusicSyncPath, string sharedLink)
        {
            
            await Task.Run(() =>
            {
                git.clone(sharedLink, songMusicSyncPath);
            });
        }

        public bool Initiated(string songMusicSyncPath)
        {
            return git.initiated(songMusicSyncPath);
        }

        public async Task UploadAllFilesAsync(string songMusicSyncPath, string title, string description)
        {
            await Task.Run(() =>
            {
                git.addAllFiles(songMusicSyncPath);
                git.commit(songMusicSyncPath, title, description);
                git.push(songMusicSyncPath);
            });
        }

        public async Task UploadFileAsync(string songMusicSyncPath, string file, string title)
        {
            await Task.Run(() =>
            {
                git.add(songMusicSyncPath, file);
                git.commit(songMusicSyncPath, title, string.Empty);
                git.push(songMusicSyncPath);
            });
        }

        public void Tag(string songMusicSyncPath, string versionNumber)
        {
            git.tag(songMusicSyncPath, versionNumber);
        }

        public async Task<bool> UpdatesAvailbleAsync(string songMusicSyncPath)
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

        public async Task DownloadLastUpdateAsync(string songMusicSyncPath)
        {
            await Task.Run(() =>
            {
                git.pull(songMusicSyncPath);
            });
        }

        public async Task RevertToLastLocalVersionAsync(string songMusicSyncPath)
        {
            await Task.Run(() =>
            {
                git.resetMasterHard(songMusicSyncPath);
            });
        }

        public async Task<SongVersion>? LastLocalVersionAsync(string songMusicSyncPath)
        {
            return await Task.Run(() =>
            {
                SongVersion currentVersion = new SongVersion();
                GitTag lastTag = git.lastLocalTag(songMusicSyncPath);
                TagToSongVersion(lastTag, currentVersion);
                return currentVersion;
            });
        }

        public async Task<List<SongVersion>>? LocalVersionsAsync(string songMusicSyncPath)
        {
            return await Task.Run(() =>
            {
                List<SongVersion> versions = new List<SongVersion>();
                var tags = git.localTags(songMusicSyncPath);
                TagsToSongVersions(tags, versions);
                return versions;
            });
        }

        public async Task<List<SongVersion>>? UpcomingVersionsAsync(string songMusicSyncPath)
        {
            return await Task.Run(() =>
            {
                List<SongVersion> upcomingVersions = new List<SongVersion>();
                var tags = git.remoteTags(songMusicSyncPath);
                TagsToSongVersions(tags, upcomingVersions);
                return upcomingVersions;
            });
        }

        public string ShareLink(string songMusicSyncPath)
        {
            return git.remoteUrl(songMusicSyncPath);
        }

        public string GuidFromSharedLink(string sharedLink)
        {
            User user = saver.savedUser();
            string bandNameFormatedForUrl = user.BandName.Replace(" ", "-");
            string UrlStart = $"{gitServerUrl}/{bandNameFormatedForUrl}/";
            string UrlEnd = ".git";
            int startPos = sharedLink.LastIndexOf(UrlStart) + UrlStart.Length;
            int length = sharedLink.IndexOf(UrlEnd) - startPos;
            string guid = sharedLink.Substring(startPos, length);
            return guid;
        }

        private static void TagsToSongVersions(List<GitTag> Tags, List<SongVersion> versions)
        {
            foreach (var tag in Tags)
            {
                SongVersion version = new SongVersion();
                TagToSongVersion(tag, version);
                versions.Add(version);
            }
        }

        private static void TagToSongVersion(GitTag lastTag, SongVersion version)
        {
            version.Number = lastTag.Name;
            version.Description = lastTag.Description.Remove(lastTag.Description.Length - 1);
            version.Author = lastTag.Author;
            version.Date = lastTag.Date;
        }

        public readonly string gitServerUrl;
        private readonly ISaver saver;
        private readonly Git git;
    }
}
