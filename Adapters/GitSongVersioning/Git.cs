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

        public void Init(string repoPath, string gitServerUrl, string repoName)
        {
            User user = saver.SavedUser();
            Repository.Init(repoPath);
            var repo = new Repository(repoPath);
            var bandNameFormatedForUrl = user.BandName.Replace(" ", "-");
            var repoNameFormatedForUrl = repoName.ToLower().Replace(" ", "-").Replace("(", null).Replace(")", null);
            string url = $"{gitServerUrl}/{ bandNameFormatedForUrl }/{ repoNameFormatedForUrl }.git";
            Remote remote = repo.Network.Remotes.Add("origin", url);
            repo.Branches.Update(repo.Head,
                b => b.Remote = remote.Name,
                b => b.UpstreamBranch = repo.Head.CanonicalName);
        }

        public bool Initiated(string repoPath)
        {
            return fileManager.DirectoryExists(repoPath + @".git");
        }

        public void Clone(string sharedLink, string downloadPath)
        {

            User user = saver.SavedUser();
            var options = new CloneOptions();
            options.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = user.BandEmail, Password = user.BandPassword, };
            Repository.Clone(sharedLink, downloadPath, options);
        }

        public int? MasterBranchIsBehindBy(string repoPath)
        {
            int? masterBranchIsBehindBy = 0;
            using (var repo = new Repository(repoPath))
            {
                string logMessage = string.Empty;
                var remote = repo.Network.Remotes["origin"];
                var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                User user = saver.SavedUser();
                FetchOptions options = new FetchOptions();
                options.CredentialsProvider = (_url, _user, _cred) =>
                     new UsernamePasswordCredentials { Username = user.BandEmail, Password = user.BandPassword, };
                Commands.Fetch(repo, remote.Name, refSpecs, options, logMessage);
                masterBranchIsBehindBy = repo.Branches["master"].TrackingDetails.BehindBy;
            }
            return masterBranchIsBehindBy;
        }

        public void AddAllFiles(string repoPath)
        {
            using (var repo = new Repository(repoPath))
            {
                Commands.Stage(repo, "*");
            }
        }

        public void Add(string repoPath, string file)
        {
            using (var repo = new Repository(repoPath))
            {
                Commands.Stage(repo, file);
            }
        }

        public void Commit(string repoPath, string title, string description)
        {
            User user = saver.SavedUser();
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

        public void Push(string repoPath)
        {
            User user = saver.SavedUser();
            using (var repo = new Repository(repoPath))
            {
                Remote remote = repo.Network.Remotes["origin"];
                var options = new PushOptions();
                options.CredentialsProvider = (_url, _user, _cred) =>
                    new UsernamePasswordCredentials { Username = user.BandEmail, Password = user.BandPassword, };
                repo.Network.Push(remote, repo.Head.CanonicalName, options);
            }
        }

        public void Pull(string repoPath)
        {
            User user = saver.SavedUser();
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

        public void ResetMasterHard(string repoPath)
        {
            using (var repo = new Repository(repoPath))
            {
                Branch originMaster = repo.Branches["origin/master"];
                repo.Reset(ResetMode.Hard, originMaster.Tip);
            }

        }

        public void Tag(string repoPath, string tag)
        {
            User user = saver.SavedUser();
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

        public GitTag LastLocalTag(string repoPath)
        {
            List<GitTag> tags = LocalTags(repoPath);
            var lastTag = tags.Last();
            return lastTag;
        }

        public List<GitTag> LocalTags(string repoPath)
        {
            List<GitTag> gitTags = new List<GitTag>();
            using (var repo = new Repository(repoPath))
            {
                List<Tag> tags = repo.Tags.ToList();
                List<Tag> tagsMerged = TagsMerged(repo, tags);
                foreach (var tag in tagsMerged)
                {
                    GitTag newTag = new GitTag(tag);
                    gitTags.Add(newTag);
                }
            }
            return gitTags;
        }

        public List<GitTag> RemoteTags(string repoPath)
        {
            List<GitTag> gitTags = new List<GitTag>();
            using (var repo = new Repository(repoPath))
            {
                List<Tag> tags = repo.Tags.ToList();
                List<Tag> tagsMerged = TagsMerged(repo, tags);
                foreach (var tag in tagsMerged)
                {
                    tags.Remove(tag);
                }
                foreach (var tag in tags)
                {
                    GitTag newTag = new GitTag(tag);
                    gitTags.Add(newTag);
                }
            }
            return gitTags;
        }

        public string RemoteUrl(string repoPath)
        {
            string remoteURL = string.Empty;
            using (var repo = new Repository(repoPath))
            {
                var remote = repo.Network.Remotes["origin"];
                remoteURL = remote.Url;
            }
            return remoteURL;
        }

        private static List<Tag> TagsMerged(Repository repo, List<Tag> tags)
        {
            List<Tag> tagsMerged = new List<Tag>();

            foreach (var tag in tags)
            {
                var commits = repo.Branches["master"].Commits;
                var tagFound = false;
                foreach (var commit in commits)
                {
                    if (tag.Target.Sha == commit.Sha)
                        tagFound = true;
                }
                if (tagFound)
                {
                    tagsMerged.Add(tag);
                }
            }

            return tagsMerged;
        }

        private readonly ISaver saver;
        private readonly IFileManager fileManager;
    }

    public class GitTag
    {
        public GitTag(Tag tag)
        {
            Name = tag.FriendlyName;
            var commit = (Commit)tag.Target;
            Description = commit.Message;
            Author = commit.Author.Name;
            DateTimeOffset date = commit.Author.When;
            Date = DateOnly.FromDateTime(date.Date);
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public DateOnly Date { get; set; }
    }
}
