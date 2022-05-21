using App1.Models;
using App1.Models.Ports;

namespace GitVersionTool
{
    public class GitTransport : ITransport
    {
        public GitTransport(string gitServerUrl, ISaver saver, IFileManager fileManager) 
        {
            GitServerUrl = gitServerUrl;
            _saver = saver;
            _git = new Git(saver, fileManager);
        }

        public void Init(string songMusicSyncPath, string name)
        {
            _git.Init(songMusicSyncPath, GitServerUrl, name);
        }

        public async Task InitAsync(string songMusicSyncPath, string sharedLink)
        {
            
            await Task.Run(() =>
            {
                _git.Clone(sharedLink, songMusicSyncPath);
            });
        }

        public bool Initiated(string songMusicSyncPath)
        {
            return _git.Initiated(songMusicSyncPath);
        }

        public async Task UploadAllFilesAsync(string songMusicSyncPath, string title, string description)
        {
            await Task.Run(() =>
            {
                _git.AddAllFiles(songMusicSyncPath);
                _git.Commit(songMusicSyncPath, title, description);
                _git.Push(songMusicSyncPath);
            });
        }

        public async Task UploadFileAsync(string songMusicSyncPath, string file, string title)
        {
            await Task.Run(() =>
            {
                _git.Add(songMusicSyncPath, file);
                _git.Commit(songMusicSyncPath, title, string.Empty);
                _git.Push(songMusicSyncPath);
            });
        }

        public void Tag(string songMusicSyncPath, string versionNumber)
        {
            _git.Tag(songMusicSyncPath, versionNumber);
        }

        public async Task<bool> UpdatesAvailbleAsync(string songMusicSyncPath)
        {
            return await Task.Run(() =>
            {
                int? behind = _git.MasterBranchIsBehindBy(songMusicSyncPath);
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
                _git.Pull(songMusicSyncPath);
            });
        }

        public async Task RevertToLastLocalVersionAsync(string songMusicSyncPath)
        {
            await Task.Run(() =>
            {
                _git.ResetMasterHard(songMusicSyncPath);
            });
        }

        public async Task<SongVersion>? LastLocalVersionAsync(string songMusicSyncPath)
        {
            return await Task.Run(() =>
            {
                SongVersion currentVersion = new SongVersion();
                GitTag lastTag = _git.LastLocalTag(songMusicSyncPath);
                TagToSongVersion(lastTag, currentVersion);
                return currentVersion;
            });
        }

        public async Task<List<SongVersion>>? LocalVersionsAsync(string songMusicSyncPath)
        {
            return await Task.Run(() =>
            {
                List<SongVersion> versions = new List<SongVersion>();
                var tags = _git.LocalTags(songMusicSyncPath);
                TagsToSongVersions(tags, versions);
                return versions;
            });
        }

        public async Task<List<SongVersion>>? UpcomingVersionsAsync(string songMusicSyncPath)
        {
            return await Task.Run(() =>
            {
                List<SongVersion> upcomingVersions = new List<SongVersion>();
                var tags = _git.RemoteTags(songMusicSyncPath);
                TagsToSongVersions(tags, upcomingVersions);
                return upcomingVersions;
            });
        }

        public string ShareLink(string songMusicSyncPath)
        {
            return _git.RemoteUrl(songMusicSyncPath);
        }

        public string GuidFromSharedLink(string sharedLink)
        {
            User user = _saver.savedUser();
            string bandNameFormatedForUrl = user.BandName.Replace(" ", "-");
            string UrlStart = $"{GitServerUrl}/{bandNameFormatedForUrl}/";
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

        public readonly string GitServerUrl;
        private readonly ISaver _saver;
        private readonly Git _git;
    }
}
