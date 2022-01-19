using System;
using System.IO;
using System.Management.Automation;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace App1
{
    internal class VersionTool
    {
        public VersionTool(User user)
        {
            USERNAME = user.gitLabUsername;
            PASSWORD = user.gitLabPassword;
            MERGE_USER_NAME = user.gitUsername;
            MERGE_USER_EMAIL = user.gitEmail;
        }

        public void addAllChanges(string directory)
        {
            using (var repo = new Repository(directory))
            {
                Commands.Stage(repo, "*");
            }
        }

        public void commitChanges(string directory, string title, string description)
        {
            using (var repo = new Repository(directory))
            {

                // Create the committer's signature and commit
                var signature = new LibGit2Sharp.Signature(
                    new Identity(MERGE_USER_NAME, MERGE_USER_EMAIL), DateTimeOffset.Now);
                LibGit2Sharp.Signature committer = signature;

                // Commit to the repository
                Commit commit = repo.Commit($"{title}\n\n{description.ReplaceLineEndings()}", signature, committer);
            }
        }

        public void pushChangesToRepo(string directory)
        {
            using (var repo = new Repository(directory))
            {
                Remote remote = repo.Network.Remotes["origin"];
                var options = new PushOptions();
                options.CredentialsProvider = (_url, _user, _cred) =>
                    new UsernamePasswordCredentials { Username = USERNAME, Password = PASSWORD };
                repo.Network.Push(remote, @"refs/heads/test_version_tool", options);
            }
        }

        public void pullChangesFromRepo(string directory)
        {
            using (var repo = new Repository(directory))
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

        public string USERNAME { get; private set; }
        public string PASSWORD { get; private set; }
        public string MERGE_USER_NAME { get; private set; }
        public string MERGE_USER_EMAIL { get; private set; }
    }
}