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
            try
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
            catch (LibGit2SharpException ex)
            {
                throw new Exception("Error when trying to init " + repoPath + " " + repoName + " : " + ex.Message);
            }
        }

        public bool initiated(string repoPath)
        {
            return fileManager.DirectoryExists(repoPath + @".git");
        }

        public void clone(string sharedLink, string downloadPath)
        {
            try
            {
                User user = saver.savedUser();
                var options = new CloneOptions();
                options.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = user.BandEmail, Password = user.BandPassword, };
                Repository.Clone(sharedLink, downloadPath, options);
            }
            catch (LibGit2SharpException ex)
            {
                throw new Exception("Error when trying to clone " + sharedLink + " : " + ex.Message);
            }
        }

        public int? masterBranchIsBehindBy(string repoPath)
        {
            try
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
            catch (LibGit2SharpException ex)
            {
                throw new Exception("Error when trying to get status on master branch for  " + repoPath + " : " + ex.Message);
            }
            
        }

        public void addAll(string repoPath)
        {
            try
            {
                using (var repo = new Repository(repoPath))
                {
                    Commands.Stage(repo, "*");
                }
            }
            catch (LibGit2SharpException ex)
            {
                throw new Exception("Error when trying to add all files for " + repoPath + " : " + ex.Message);
            }

        }

        public void add(string repoPath, string file)
        {
            try
            {
                using (var repo = new Repository(repoPath))
                {
                    Commands.Stage(repo, file);
                }
            }
            catch (LibGit2SharpException ex)
            {
                throw new Exception("Error when trying to add all files for " + repoPath + " and file " + file + " : " + ex.Message);
            } 
        }

        public void commit(string repoPath, string title, string description)
        {
            try
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
            catch (LibGit2SharpException ex)
            {
                throw new Exception("Error when trying to commit " + repoPath +  " : " + ex.Message);
            }
        }

        public void push(string repoPath)
        {
            try
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
            catch (LibGit2SharpException ex)
            {
                throw new Exception("Error when trying to push " + repoPath + " : " + ex.Message);
            }
        }

        public void pull(string repoPath)
        {
            try
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
            catch (LibGit2SharpException ex)
            {
                throw new Exception("Error when trying to pull " + repoPath + " : " + ex.Message);
            }
        }

        public void resetMasterHard(string repoPath)
        {
            try
            {
                using (var repo = new Repository(repoPath))
                {
                    Branch originMaster = repo.Branches["origin/master"];
                    repo.Reset(ResetMode.Hard, originMaster.Tip);
                }
            }
            catch (LibGit2SharpException ex)
            {
                throw new Exception("Error when trying to reset master branch for " + repoPath + " : " + ex.Message);
            }  
        }

        public void tag(string repoPath, string tag)
        {
            try
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
            catch (LibGit2SharpException ex)
            {
                throw new Exception("Error when trying to tag " + repoPath + " : " + ex.Message);
            }
        }

        public Tag lastTag(string repoPath)
        {
            try
            {
                var repo = new Repository(repoPath);
                return repo.Tags.Last();
            }
            catch (LibGit2SharpException ex)
            {
                throw new Exception("Error when trying to get last tag for " + repoPath + " : " + ex.Message);
            }
        }

        public TagCollection tags(string repoPath)
        {
            try
            {
                var repo = new Repository(repoPath);
                return repo.Tags;
            }
            catch (LibGit2SharpException ex)
            {
                throw new Exception("Error when trying to get tags for " + repoPath + " : " + ex.Message);
            }
        }

        public string remoteUrl(string repoPath)
        {
            try
            {
                string remoteURL = string.Empty;
                using (var repo = new Repository(repoPath))
                {
                    var remote = repo.Network.Remotes["origin"];
                    remoteURL = remote.Url;
                }      
                return remoteURL;
            }
            catch (LibGit2SharpException ex)
            {
                throw new Exception("Error when trying to get remote URL for " + repoPath + " : " + ex.Message);
            }
        }

        private ISaver saver;
        private IFileManager fileManager;
    }
}
