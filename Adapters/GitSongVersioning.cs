﻿using App1.Models;
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

        public async Task<string> versionDescriptionAsync(Song song)
        {
            string songVersionDescription = string.Empty;
            await Task.Run(() =>
            {
                using (var repo = new Repository(song.LocalPath))
                {
                    Commit commitTagged = (Commit)repo.Tags.Last().Target;
                    songVersionDescription = commitTagged.Message;
                }
            });
            return songVersionDescription;
        }

        public async Task<string> versionNumberAsync(Song song)
        {
            string versionNumber =string.Empty;
            await Task.Run(() =>
            {
                try
                {
                    using (var repo = new Repository(song.LocalPath))
                    {
                        versionNumber = repo.Tags.Last().FriendlyName;
                    }
                }
                catch (Exception)
                {
                    versionNumber = string.Empty;
                }
                
            });
            return versionNumber;
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
                    new UsernamePasswordCredentials { Username = user.GitLabUsername, Password = user.GitLabPassword, };
                repo.ApplyTag(tag);
                repo.Network.Push(remote, @"refs/tags/" + tag, options);
            }
        }
    }
}
