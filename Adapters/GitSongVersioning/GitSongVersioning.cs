using App1.Models;
using App1.Models.Ports;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System.IO.Compression;

namespace GitVersionTool
{
    public class GitSongVersioning : IVersionTool
    {
        public GitSongVersioning(string askedMusiSyncFolderLocation, ISaver newSaver, IFileManager newFileManager) 
        {
            musicSyncFolder = string.Empty;
            createMusicSyncFolder(askedMusiSyncFolderLocation);
            saver = newSaver;
            fileManager = newFileManager;
        }

        public async Task<string> uploadSongAsync(Song song, string title, string description, string versionNumber)
        {
            try
            {
                if (!repoInitiated(song))
                {
                    initiateRepo(song);
                }
                await compressSongAsync(song);
                await Task.Run(() =>
                {
                    addAllChanges(song);
                    commitChanges(song, title, description);
                    pushChangesToRepo(song);
                    tagRepo(song, versionNumber);
                });
                return String.Empty;
            }
            catch (LibGit2SharpException ex)
            {
                return ex.Message;
            }
            catch (Exception ex)
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
                    manageFile(song, file);
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
            string errorMessage = await updateSongFromRepoAsync(song);
            if (errorMessage != String.Empty)
            {
                return errorMessage;
            }
            await uncompressSongAsync(song);
            manageLockFile(song);
            return String.Empty;
        }

        public async Task<string> revertSongAsync(Song song)
        {
            string errorMessage = await revertSongFromRepoAsync(song);
            if (errorMessage != String.Empty)
            {
                return errorMessage;
            }
            await uncompressSongAsync(song);
            manageLockFile(song);
            return String.Empty;
        }

        public async Task<SongVersion> currentVersionAsync(Song song)
        {
            SongVersion currentVersion = new SongVersion();
            await Task.Run(() =>
            {
                try
                {
                    using (var repo = new Repository(getRepoPath(song)))
                    {
                        Tag lastTag = repo.Tags.Last();
                        currentVersion.Number = lastTag.FriendlyName;
                        Commit commitTagged = (Commit)lastTag.Target;
                        currentVersion.Description = commitTagged.Message.Remove(commitTagged.Message.Length - 1); ;
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
                    using (var repo = new Repository(getRepoPath(song)))
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

        public async Task<string> downloadSharedSongAsync(string songFolder, string sharedLink, string downloadLocalPath)
        {
            string repoPath = getRepoPath(songFolder);
            string errorMessage = await downloadSharedSongFromRepoAsync(sharedLink, repoPath);
            if (errorMessage != String.Empty)
            {
                return errorMessage;
            }
            await uncompressSongAsync(songFolder, downloadLocalPath, repoPath);
            return String.Empty;
        }

        public async Task<string> shareSongAsync(Song song)
        {
            try
            {
                return await Task.Run(() =>
                {
                    using (var repo = new Repository(getRepoPath(song)))
                    {
                        var remote = repo.Network.Remotes["origin"];
                        return remote.PushUrl;
                    }
                });

            }
            catch (LibGit2SharpException ex)
            {
                return ex.Message;
            }
        }

        private bool repoInitiated(Song song)
        {
            if (Directory.Exists(getRepoPath(song) + @".git"))
            {
                return true;
            }
            return false;
        }

        private void initiateRepo(Song song)
        {
            User user = saver.savedUser();
            Repository.Init(getRepoPath(song));
            var repo = new Repository(getRepoPath(song));
            string url = "https://gitlab.com/" + user.BandName.Replace(" ", "-") + "/" + song.Title.ToLower().Replace(" ", "-").Replace("(", null).Replace(")", null) + ".git";
            Remote remote = repo.Network.Remotes.Add("origin", url);
            repo.Branches.Update(repo.Head,
                b => b.Remote = remote.Name,
                b => b.UpstreamBranch = repo.Head.CanonicalName);
        }

        private async Task<string> updateSongFromRepoAsync(Song song)
        {
            try
            {
                await Task.Run(() =>
                {
                    User user = saver.savedUser();
                    using (var repo = new Repository(getRepoPath(song)))
                    {
                        PullOptions options = new PullOptions();
                        options.FetchOptions = new FetchOptions();
                        options.FetchOptions.CredentialsProvider = new CredentialsHandler((url, usernameFromUrl, types) =>
                                new UsernamePasswordCredentials()
                                { Username = user.BandEmail, Password = user.BandPassword });

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

        private async Task<string> downloadSharedSongFromRepoAsync(string sharedLink, string downloadPath)
        {
            try
            {
                await Task.Run(() =>
                {
                    
                    User user = saver.savedUser();
                    var options = new CloneOptions();
                    options.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = user.BandEmail, Password = user.BandPassword, };
                    Repository.Clone(sharedLink, downloadPath, options);
                });
                return string.Empty;
            }
            catch (LibGit2SharpException ex)
            {
                return ex.Message;
            }
        }

        private async Task<string> revertSongFromRepoAsync(Song song)
        {
            try
            {
                await Task.Run(() =>
                {
                    using (var repo = new Repository(getRepoPath(song)))
                    {
                        Branch originMaster = repo.Branches["origin/master"];
                        repo.Reset(ResetMode.Hard, originMaster.Tip);
                    }
                });
                return String.Empty;
            }
            catch (LibGit2SharpException ex)
            {
                return ex.Message;
            }
        }

        private async Task compressSongAsync(Song song)
        {
            if (File.Exists(getRepoPath(song) + song.Title + ".zip"))
            {
                File.Delete(getRepoPath(song) + song.Title + ".zip");
            }
            string pathToSongWithSelectedFodlers = await selectFoldersToBeCompressed(song);
            await fileManager.CompressDirectoryAsync(pathToSongWithSelectedFodlers, song.Title + ".zip", getRepoPath(song));
            Directory.Delete(pathToSongWithSelectedFodlers, true);
        }

        private async Task<string> selectFoldersToBeCompressed(Song song)
        {
            string tmpDirectory = song.LocalPath + @"\tmpDirectory";
            if (Directory.Exists(tmpDirectory))
            {
                Directory.Delete(tmpDirectory, true);
            }
            Directory.CreateDirectory(tmpDirectory);

            string MediaFolderSrc = song.LocalPath + @"\Media";
            string MediaFolderDst = tmpDirectory + @"\Media";
            string MelodyneFolderSrc = song.LocalPath + @"\Melodyne";
            string MelodyneFolderDst = tmpDirectory + @"\Melodyne";

            string songFile = await fileManager.findFileNameBasedOnExtensionAsync(song.LocalPath, ".song");
            await fileManager.CopyFileAsync(songFile, song.LocalPath, tmpDirectory);
            
            if (Directory.Exists(MediaFolderSrc))
            {
                fileManager.CopyDirectory(MediaFolderSrc, MediaFolderDst, true);
            }

            if (Directory.Exists(MelodyneFolderSrc))
            {
                fileManager.CopyDirectory(MelodyneFolderSrc, MelodyneFolderDst, true);
            }
                
            return tmpDirectory;
        }

        

        private async Task uncompressSongAsync(Song song)
        {
            await Task.Run(() =>
            {
                if (Directory.Exists(song.LocalPath))
                {
                    Directory.Delete(song.LocalPath, true);
                }
            });

            string repoPath = getRepoPath(song);
            string zipFile = await fileManager.findFileNameBasedOnExtensionAsync(repoPath, ".zip");
            await fileManager.UncompressArchiveAsync(repoPath + zipFile, song.LocalPath);
        }

        private async Task uncompressSongAsync(string songFolder, string downloadLocalPath, string repoPath)
        {
            string zipFile = await fileManager.findFileNameBasedOnExtensionAsync(repoPath, ".zip");
            await fileManager.UncompressArchiveAsync(repoPath + zipFile, downloadLocalPath + @"\" + songFolder);
        }

        private void manageFile(Song song, string file)
        {
            if (File.Exists(song.LocalPath + @"\" + file))
            {
                File.Copy(song.LocalPath + @"\" + file, getRepoPath(song) + file);
            }
            else
            {
                File.Delete(getRepoPath(song) + file);
            }
        }

        private void manageLockFile(Song song)
        {
            if (File.Exists(getRepoPath(song) + @"\.lock" ))
            {
                File.Copy(getRepoPath(song) + @"\.lock", song.LocalPath + @"\.lock");
            }
            else
            {
                File.Delete(song.LocalPath + @"\.lock");
            }
        }

        private void addAllChanges(Song song)
        {
            using (var repo = new Repository(getRepoPath(song)))
            {
                Commands.Stage(repo, "*");
            }
        }

        private void addChanges(Song song, string file)
        {
            using (var repo = new Repository(getRepoPath(song)))
            {
                Commands.Stage(repo, file);
            }
        }

        private void commitChanges(Song song, string title, string description)
        {
            
            User user = saver.savedUser();
            using (var repo = new Repository(getRepoPath(song)))
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
            User user = saver.savedUser();
            using (var repo = new Repository(getRepoPath(song)))
            {
                Remote remote = repo.Network.Remotes["origin"];
                var options = new PushOptions();
                options.CredentialsProvider = (_url, _user, _cred) =>
                    new UsernamePasswordCredentials { Username = user.BandEmail, Password = user.BandPassword, };
                repo.Network.Push(remote, repo.Head.CanonicalName, options);
            }
        }

        private void tagRepo(Song song, string tag)
        {
            User user = saver.savedUser();
            using (var repo = new Repository(getRepoPath(song)))
            {
                Remote remote = repo.Network.Remotes["origin"];
                var options = new PushOptions();
                options.CredentialsProvider = (_url, _user, _cred) =>
                    new UsernamePasswordCredentials { Username = user.BandEmail, Password = user.BandPassword, };
                repo.ApplyTag(tag);
                repo.Network.Push(remote, @"refs/tags/" + tag, options);
            }
        }

        private string getRepoPath(Song song)
        {
            return musicSyncFolder + song.Title + @"\";
        }

        private string getRepoPath(string title)
        {
            return musicSyncFolder + title + @"\";
        }

        private void createMusicSyncFolder(string askedMusicSyncFolderLocation)
        {
            if (!string.IsNullOrEmpty(askedMusicSyncFolderLocation))
            {
                musicSyncFolder = askedMusicSyncFolderLocation + @"\.musicsync\";
                Directory.CreateDirectory(musicSyncFolder);
            }
        }

        private string musicSyncFolder;
        private ISaver saver;
        private IFileManager fileManager;
    }
}
