using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.IO;

namespace App1
{
    public class VersionTool
    {
        public VersionTool()
        {
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
            User user = savedUser();
            string username = File.ReadAllText(song.localPath + @"\.lock");
            if (username == user.gitUsername)
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
            User user = savedUser();
            File.WriteAllText(song.localPath + @"\.lock", user.gitUsername);
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
            User user = savedUser();
            using (var repo = new Repository(song.localPath))
            {
                // Credential information to fetch
                PullOptions options = new PullOptions();
                options.FetchOptions = new FetchOptions();
                options.FetchOptions.CredentialsProvider = new CredentialsHandler(
                    (url, usernameFromUrl, types) =>
                        new UsernamePasswordCredentials()
                        {
                            Username = user.gitLabUsername,
                            Password = user.gitLabPassword
                        });

                // User information to create a merge commit
                var signature = new Signature(
                    new Identity(user.gitUsername, user.gitEmail), DateTimeOffset.Now);

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
            User user = savedUser();
            using (var repo = new Repository(song.localPath))
            {
                // Create the committer's signature and commit
                var signature = new Signature(
                    new Identity(user.gitUsername, user.gitEmail), DateTimeOffset.Now);
                Signature committer = signature;

                // Commit to the repository
                Commit commit = repo.Commit($"{title}\n\n{description.ReplaceLineEndings()}", signature, committer);
            }
        }

        private void pushChangesToRepo(Song song)
        {
            User user = savedUser();
            using (var repo = new Repository(song.localPath))
            {
                Remote remote = repo.Network.Remotes["origin"];
                var options = new PushOptions();
                options.CredentialsProvider = (_url, _user, _cred) =>
                    new UsernamePasswordCredentials { Username = user.gitLabUsername, Password = user.gitLabPassword, };
                repo.Network.Push(remote, @"refs/heads/master", options);
            }
        }

        private static User savedUser()
        {
            Saver saver = new Saver();
            User user = saver.savedUser();
            return user;
        }
    }
}