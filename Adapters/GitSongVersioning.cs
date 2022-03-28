using App1.Models;
using App1.Models.Ports;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace App1.Adapters
{
    public class GitSongVersioning : IVersionTool
    {
        public GitSongVersioning() { }

        public async Task<string> uploadSongAsync(Song song, string title, string description, string versionNumber)
        {
            try
            {
                await Task.Run(() =>
                {
                    addAllChanges(song);
                    commitChanges(song, title, description);
                    pushChangesToRepo(song);
                    tagRepo(song, versionNumber);
                });
                return String.Empty;
            }
            catch(LibGit2SharpException ex)
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
                    addChanges(song, file);
                    commitChanges(song, title, string.Empty);
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
                                {Username = user.BandEmail, Password = user.BandPassword});

                        var signature = new Signature(new Identity(user.Username, user.BandEmail), DateTimeOffset.Now);

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
                        Branch originMaster = repo.Branches["origin/master"];
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

        public async Task<SongVersion> currentVersionAsync(Song song)
        {
            SongVersion currentVersion = new SongVersion();
            await Task.Run(() =>
            {
                try
                {
                    using (var repo = new Repository(song.LocalPath))
                    {
                        Tag lastTag = repo.Tags.Last();
                        currentVersion.Number = lastTag.FriendlyName;
                        Commit commitTagged = (Commit)lastTag.Target;
                        currentVersion.Description = commitTagged.Message;
                        currentVersion.Author = commitTagged.Author.Name;
                    }
                }
                catch (Exception)
                {
                    currentVersion = new SongVersion();
                }
                
            });
            return currentVersion;
        }

        public async Task<List<SongVersion>> versionsAsync(Song song)
        {
            
            List<SongVersion> versions = new List<SongVersion>();
            await Task.Run(() =>
            {
                try
                {
                    using (var repo = new Repository(song.LocalPath))
                    {
                        foreach (var tag in repo.Tags)
                        {
                            SongVersion version = new SongVersion();
                            version.Number = tag.FriendlyName;
                            Commit commitTagged = (Commit)tag.Target;
                            version.Description = commitTagged.Message;
                            version.Author = commitTagged.Author.Name;
                            versions.Add(version);
                        }
                    }
                }
                catch (Exception)
                {
                    versions = new List<SongVersion>();
                }

            });
            return versions;
        }

        public async Task<string> downloadSharedSongAsync(string linkShared, string downloadLocalPath)
        {
            try
            {
                await Task.Run(() =>
                {
                    ISaver saver = new LocalSettingsSaver();
                    User user = saver.savedUser();
                    var options = new CloneOptions();
                    options.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = user.BandEmail, Password = user.BandPassword, };
                    Repository.Clone(linkShared, downloadLocalPath, options);
                });
                return string.Empty;
            }
            catch (LibGit2SharpException ex)
            {
                return ex.Message;
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
                    new Identity(user.Username, user.BandEmail), DateTimeOffset.Now);
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
                    new UsernamePasswordCredentials { Username = user.BandEmail, Password = user.BandPassword, };
                repo.Network.Push(remote, @"refs/heads/master", options);
            }
        }

        private void tagRepo(Song song, string tag)
        {
            ISaver saver = new LocalSettingsSaver();
            User user = saver.savedUser();
            using (var repo = new Repository(song.LocalPath))
            {
                Remote remote = repo.Network.Remotes["origin"];
                var options = new PushOptions();
                options.CredentialsProvider = (_url, _user, _cred) =>
                    new UsernamePasswordCredentials { Username = user.BandEmail, Password = user.BandPassword, };
                repo.ApplyTag(tag);
                repo.Network.Push(remote, @"refs/tags/" + tag, options);
            }
        }
    }
}
