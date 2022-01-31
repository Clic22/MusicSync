using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.IO;

namespace App1
{
    public class VersionTool
    {
        public VersionTool(User user)
        {
            USERNAME = user.gitLabUsername;
            PASSWORD = user.gitLabPassword;
            MERGE_USER_NAME = user.gitUsername;
            MERGE_USER_EMAIL = user.gitEmail;
        }

        public void addCommitAndPush(Song song, string title, string description)
        {
            addAllChanges(song);
            commitChanges(song, title, description);
            pushChangesToRepo(song);
        }

        public void updateSongStatus(Song song)
        {
            if (lockFileExist(song))
            {
                song.status = Song.SongStatus.locked;
            }
            else
            {
                song.status = Song.SongStatus.upToDate;
            }
        }

        internal bool isLockedByUser(Song song)
        {
            if (lockFileExist(song))
            {
                if (lockFileCreatedByUser(song))
                    return true;
            }
            return false;

        }

        private bool lockFileCreatedByUser(Song song)
        {
            string username = File.ReadAllText(song.localPath + @"\.lock");
            if (username == MERGE_USER_NAME)
            {
                return true;
            }
            return false;
        }

        public void lockSong(Song song)
        {
            createLockFile(song);
            uploadLock(song);
            song.status = Song.SongStatus.locked;
        }

        public void unlockSong(Song song)
        {
            removeLockFile(song);
            uploadUnlock(song);
            song.status = Song.SongStatus.upToDate;
        }

        private void createLockFile(Song song)
        {
            File.WriteAllText(song.localPath + @"\.lock", MERGE_USER_NAME);
        }

        private void removeLockFile(Song song)
        {
            File.Delete(song.localPath + @"\.lock");
        }

        private void uploadLock(Song song)
        {
            addLockCommitAndPush(song, "lock");
        }

        private void uploadUnlock(Song song)
        {
            addLockCommitAndPush(song, "unlock");
        }


        private bool lockFileExist(Song song)
        {
            if (File.Exists(song.localPath + @"\.lock"))
            {
                return true;
            }
            return false;
        }

        public void addLockCommitAndPush(Song song, string title)
        {
            addLockChanges(song);
            commitChanges(song, title, string.Empty);
            pushChangesToRepo(song);
        }

        public void revertChanges(Song song)
        {
            using (var repo = new Repository(song.localPath))
            {
                Branch originMaster = repo.Branches["origin/master"];
                repo.Reset(ResetMode.Hard, originMaster.Tip);
            }
        }

        public void pullChangesFromRepo(Song song)
        {
            using (var repo = new Repository(song.localPath))
            {
                // Credential information to fetch
                LibGit2Sharp.PullOptions options = new LibGit2Sharp.PullOptions();
                options.FetchOptions = new FetchOptions();
                options.FetchOptions.CredentialsProvider = new CredentialsHandler(
                    (url, usernameFromUrl, types) =>
                        new UsernamePasswordCredentials()
                        {
                            Username = USERNAME,
                            Password = PASSWORD
                        });

                // User information to create a merge commit
                var signature = new LibGit2Sharp.Signature(
                    new Identity(MERGE_USER_NAME, MERGE_USER_EMAIL), DateTimeOffset.Now);

                // Pull
                Commands.Pull(repo, signature, options);
            }
        }

        private void addAllChanges(Song song)
        {
            using (var repo = new Repository(song.localPath))
            {
                Commands.Stage(repo, "*");
            }
        }

        private void addLockChanges(Song song)
        {
            using (var repo = new Repository(song.localPath))
            {
                Commands.Stage(repo, ".lock");
            }
        }

        private void commitChanges(Song song, string title, string description)
        {
            using (var repo = new Repository(song.localPath))
            {

                // Create the committer's signature and commit
                var signature = new LibGit2Sharp.Signature(
                    new Identity(MERGE_USER_NAME, MERGE_USER_EMAIL), DateTimeOffset.Now);
                LibGit2Sharp.Signature committer = signature;

                // Commit to the repository
                Commit commit = repo.Commit($"{title}\n\n{description.ReplaceLineEndings()}", signature, committer);
            }
        }

        private void pushChangesToRepo(Song song)
        {
            using (var repo = new Repository(song.localPath))
            {
                Remote remote = repo.Network.Remotes["origin"];
                var options = new PushOptions();
                options.CredentialsProvider = (_url, _user, _cred) =>
                    new UsernamePasswordCredentials { Username = USERNAME, Password = PASSWORD };
                repo.Network.Push(remote, @"refs/heads/master", options);
            }
        }

        public string USERNAME { get; private set; }
        public string PASSWORD { get; private set; }
        public string MERGE_USER_NAME { get; private set; }
        public string MERGE_USER_EMAIL { get; private set; }
    }
}