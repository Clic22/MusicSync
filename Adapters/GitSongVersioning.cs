using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1.Adapters
{
    public class GitSongVersioning : IVersionTool
    {
        public GitSongVersioning() { }

        public async Task uploadSongAsync(Song song, string title, string description)
        {
            await Task.Run(() =>
            {
                addAllChanges(song);
                commitChanges(song, title, description);
                pushChangesToRepo(song);
            });
        }

        public async Task updateSongAsync(Song song)
        {
            await Task.Run(() =>
            {
                Saver saver = new Saver();
                User user = saver.savedUser();
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
            });
        }

        public async Task revertSongAsync(Song song)
        {
            await Task.Run(() =>
            {
                using (var repo = new Repository(song.localPath))
                {
                    Branch originMaster = repo.Branches["origin/master"];
                    repo.Reset(ResetMode.Hard, originMaster.Tip);
                }
            });
        }

        private void addAllChanges(Song song)
        {
            using (var repo = new Repository(song.localPath))
            {
                Commands.Stage(repo, "*");
            }
        }

        private void commitChanges(Song song, string title, string description)
        {
            Saver saver = new Saver();
            User user = saver.savedUser();
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
            Saver saver = new Saver();
            User user = saver.savedUser();
            using (var repo = new Repository(song.localPath))
            {
                Remote remote = repo.Network.Remotes["origin"];
                var options = new PushOptions();
                options.CredentialsProvider = (_url, _user, _cred) =>
                    new UsernamePasswordCredentials { Username = user.gitLabUsername, Password = user.gitLabPassword, };
                repo.Network.Push(remote, @"refs/heads/master", options);
            }
        }
    }
}
