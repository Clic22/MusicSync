using App1.Models;
using App1.Models.Ports;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace App1.Adapters
{
    public class GitSongVersioning : IVersionTool
    {
        public GitSongVersioning() { }

        public async Task<string> uploadSongAsync(Song song, string title, string description)
        {
            try
            {
                await Task.Run(() =>
                {
                    addAllChanges(song);
                    commitChanges(song, title, description);
                    pushChangesToRepo(song);
                });
                return String.Empty;
            }
            catch(LibGit2SharpException ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> uploadSongAsync(Song song, string file, string title, string description)
        {
            try
            {
                await Task.Run(() =>
                {
                    addChanges(song, file);
                    commitChanges(song, title, description);
                    pushChangesToRepo(song);
                });
                return String.Empty;
            }
            catch (LibGit2SharpException ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> updateSongAsync(Song song)
        {
            try
            {
                await Task.Run(() =>
                {
                    ISaver saver = new LocalSettingsSaver();
                    User user = saver.savedUser();
                    using (var repo = new Repository(song.LocalPath))
                    {
                        PullOptions options = new PullOptions();
                        options.FetchOptions = new FetchOptions();
                        options.FetchOptions.CredentialsProvider = new CredentialsHandler((url, usernameFromUrl, types) =>
                                new UsernamePasswordCredentials()
                                {Username = user.GitLabUsername, Password = user.GitLabPassword});

                        var signature = new Signature(new Identity(user.GitUsername, user.GitEmail), DateTimeOffset.Now);

                        Commands.Pull(repo, signature, options);
                    }
                });
                return String.Empty;
            }
            catch (LibGit2SharpException ex)
            {
                return ex.Message;
            }
            catch (ArgumentException ex)
            {
                return ex.Message;
            }
            
        }

        public async Task<string> revertSongAsync(Song song)
        {
            try
            {
                await Task.Run(() =>
                {
                    using (var repo = new Repository(song.LocalPath))
                    {
                        Branch originMaster = repo.Branches["origin/test_version_tool"];
                        repo.Reset(ResetMode.Hard, originMaster.Tip);
                    }
                });
                return String.Empty;
            }
            catch(LibGit2SharpException ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> songVersionDescriptionAsync(Song song)
        {
            string songVersionDescription = string.Empty;
            try
            {
                await Task.Run(() =>
                {
                    using (var repo = new Repository(song.LocalPath))
                    {
                        Tag songVersionNumber = repo.Tags.Last();
                        songVersionDescription = ((Commit)songVersionNumber.Target).Message;
                    }
                });
                return songVersionDescription;
            }
            catch 
            {
                return songVersionDescription;
            }
        }

        public async Task<string> songVersionNumberAsync(Song song)
        {
            string songVersionNumber = string.Empty;
            try
            {
                await Task.Run(() =>
                {
                    using (var repo = new Repository(song.LocalPath))
                    {
                        songVersionNumber = repo.Tags.Last().FriendlyName;
                    }
                });
                return songVersionNumber;
            }
            catch
            {
                return songVersionNumber;
            }
        }

        private void addAllChanges(Song song)
        {
            using (var repo = new Repository(song.LocalPath))
            {
                Commands.Stage(repo, "*");
            }
        }

        private void addChanges(Song song, string file)
        {
            using (var repo = new Repository(song.LocalPath))
            {
                Commands.Stage(repo, file);
            }
        }

        static private void commitChanges(Song song, string title, string description)
        {
            ISaver saver = new LocalSettingsSaver();
            User user = saver.savedUser();
            using (var repo = new Repository(song.LocalPath))
            {
                var signature = new Signature(
                    new Identity(user.GitUsername, user.GitEmail), DateTimeOffset.Now);
                Signature committer = signature;
                if (string.IsNullOrEmpty(description))
                {
                    repo.Commit($"{title}", signature, committer);
                }
                else
                {
                    repo.Commit($"{title}\n\n{description.ReplaceLineEndings()}", signature, committer);
                }
            }
        }

        private void pushChangesToRepo(Song song)
        {
            ISaver saver = new LocalSettingsSaver();
            User user = saver.savedUser();
            using (var repo = new Repository(song.LocalPath))
            {
                Remote remote = repo.Network.Remotes["origin"];
                var options = new PushOptions();
                options.CredentialsProvider = (_url, _user, _cred) =>
                    new UsernamePasswordCredentials { Username = user.GitLabUsername, Password = user.GitLabPassword, };
                repo.Network.Push(remote, @"refs/heads/test_version_tool", options);
            }
        }

        private void tagRepo(Song song, string versionNumber)
        {
            using (var repo = new Repository(song.LocalPath))
            {
                repo.ApplyTag(versionNumber);
            }
        }

    }
}
