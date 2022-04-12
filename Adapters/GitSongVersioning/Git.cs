using App1.Models;
using App1.Models.Ports;
using LibGit2Sharp;

namespace GitVersionTool
{
    public class Git
    {
        public Git(ISaver saver, IFileManager fileMnager)
        {
            this.saver = saver;
            fileManager = fileMnager;
        }

        public void init(string repoPath, string repoName)
        {

            User user = saver.savedUser();
            Repository.Init(repoPath);
            var repo = new Repository(repoPath);
            string url = "https://gitlab.com/" + user.BandName.Replace(" ", "-") + "/" + repoName.ToLower().Replace(" ", "-").Replace("(", null).Replace(")", null) + ".git";
            Remote remote = repo.Network.Remotes.Add("origin", url);
            repo.Branches.Update(repo.Head,
                b => b.Remote = remote.Name,
                b => b.UpstreamBranch = repo.Head.CanonicalName);

        }

        public bool initiated(string repoPath)
        {
            return fileManager.DirectoryExists(repoPath + @".git");
        }

        public void clone(string sharedLink, string downloadPath)
        {

            User user = saver.savedUser();
            var options = new CloneOptions();
            options.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = user.BandEmail, Password = user.BandPassword, };
            Repository.Clone(sharedLink, downloadPath, options);

        }

        public int? masterBranchIsBehindBy(string repoPath)
        {

            int? masterBranchIsBehindBy = 0;
            using (var repo = new Repository(repoPath))
            {
                string logMessage = string.Empty;
                var remote = repo.Network.Remotes["origin"];
                var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                User user = saver.savedUser();
                FetchOptions options = new FetchOptions();
                options.CredentialsProvider = (_url, _user, _cred) =>
                     new UsernamePasswordCredentials { Username = user.BandEmail, Password = user.BandPassword, };
                Commands.Fetch(repo, remote.Name, refSpecs, options, logMessage);
                masterBranchIsBehindBy = repo.Branches["master"].TrackingDetails.BehindBy;
            }
            return masterBranchIsBehindBy;


        }

        public void addAll(string repoPath)
        {
            using (var repo = new Repository(repoPath))
            {
                Commands.Stage(repo, "*");
            }
        }

        public void add(string repoPath, string file)
        {
            using (var repo = new Repository(repoPath))
            {
                Commands.Stage(repo, file);
            }
        }

        public void commit(string repoPath, string title, string description)
        {
            User user = saver.savedUser();
            using (var repo = new Repository(repoPath))
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

        public void push(string repoPath)
        {
            User user = saver.savedUser();
            using (var repo = new Repository(repoPath))
            {
                Remote remote = repo.Network.Remotes["origin"];
                var options = new PushOptions();
                options.CredentialsProvider = (_url, _user, _cred) =>
                    new UsernamePasswordCredentials { Username = user.BandEmail, Password = user.BandPassword, };
                repo.Network.Push(remote, repo.Head.CanonicalName, options);
            }
        }

        public void pull(string repoPath)
        {
            User user = saver.savedUser();
            using (var repo = new Repository(repoPath))
            {
                PullOptions options = new PullOptions();
                options.FetchOptions = new FetchOptions();
                options.FetchOptions.CredentialsProvider = (_url, _user, _cred) =>
                     new UsernamePasswordCredentials { Username = user.BandEmail, Password = user.BandPassword, };

                var signature = new Signature(new Identity(user.Username, user.BandEmail), DateTimeOffset.Now);

                Commands.Pull(repo, signature, options);
            }
        }

        public void resetMasterHard(string repoPath)
        {
            using (var repo = new Repository(repoPath))
            {
                Branch originMaster = repo.Branches["origin/master"];
                repo.Reset(ResetMode.Hard, originMaster.Tip);
            }

        }

        public void tag(string repoPath, string tag)
        {
            User user = saver.savedUser();
            using (var repo = new Repository(repoPath))
            {
                Remote remote = repo.Network.Remotes["origin"];
                var options = new PushOptions();
                options.CredentialsProvider = (_url, _user, _cred) =>
                    new UsernamePasswordCredentials { Username = user.BandEmail, Password = user.BandPassword, };
                repo.ApplyTag(tag);
                repo.Network.Push(remote, @"refs/tags/" + tag, options);
            }
        }

        public Tag lastTag(string repoPath)
        {
            var repo = new Repository(repoPath);
            return repo.Tags.Last();
        }

        public TagCollection tags(string repoPath)
        {

            var repo = new Repository(repoPath);
            return repo.Tags;

        }

        public string remoteUrl(string repoPath)
        {

            string remoteURL = string.Empty;
            using (var repo = new Repository(repoPath))
            {
                var remote = repo.Network.Remotes["origin"];
                remoteURL = remote.Url;
            }
            return remoteURL;
        }

        private readonly ISaver saver;
        private readonly IFileManager fileManager;
    }
}
