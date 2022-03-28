using App1.Models;
using App1.Models.Ports;


namespace App1Tests.Mock
{
    public class VersioningMock : IVersionTool
    {
        public VersioningMock(User user)
        {
            this.user = user;
            string Username1 = "Hear@fdjskjè_";
            string BandPassword1 = "12df546@";
            string BandName1 = "Clic5456";
            string BandEmail1 = "testdklsjfhg@yahoo.com";
            user1 = new User(BandName1, BandPassword1, Username1, BandEmail1);
            string Username2 = "Lithorama52";
            string BandPassword2 = "15@^_usnjdfb@";
            string BandName2 = "Erratum12";
            string BandEmail2 = "erratum12@gmail.com";
            user2 = new User(BandName2, BandPassword2, Username2, BandEmail2);
            versionPath = @"./versionStorage/";
            Directory.CreateDirectory(versionPath);
            currentVersion = new SongVersion();
            versions = new List<SongVersion>();
        }

        public async Task<string> uploadSongAsync(Song song, string title, string description, string versionNumber)
        {
            (bool errorBool, string errorMessage) = await UserErrorAsync();

            if (!errorBool && song.LocalPath != null)
            {
                if (Directory.Exists(versionPath + song.LocalPath))
                {
                    Directory.Delete(versionPath + song.LocalPath, true);
                }
                Copy(song.LocalPath, versionPath + song.LocalPath);
                currentVersion.Number = versionNumber;
                currentVersion.Description = title + "\n\n" + description;
                currentVersion.Author = user.Username;
                SongVersion songVersion = new SongVersion(versionNumber, title + "\n\n" + description, user.Username);
                versions.Add(songVersion);
            }
            return errorMessage;
        }

        public async Task<string> uploadSongAsync(Song song, string file, string title)
        {
            (bool errorBool, string errorMessage) = await UserErrorAsync();
            if (!errorBool && song.LocalPath != null)
            {
                if (File.Exists(versionPath + song.LocalPath + file))
                {
                    File.Delete(versionPath + song.LocalPath + file);
                }
                if (File.Exists(song.LocalPath + file))
                {
                    if (!Directory.Exists(versionPath + song.LocalPath))
                    {
                        Directory.CreateDirectory(versionPath + song.LocalPath);
                    }
                    File.Copy(song.LocalPath + file, versionPath + song.LocalPath + file);
                }
            }
            return errorMessage;
        }

        public async Task<string> updateSongAsync(Song song)
        {
            (bool errorBool, string errorMessage) = await UserErrorAsync();
            if (!errorBool && song.LocalPath != null)
            {
                Copy(versionPath + song.LocalPath, song.LocalPath);
            }
            return errorMessage;
        }

        public async Task<string> revertSongAsync(Song song)
        {
            (bool errorBool, string errorMessage) = await UserErrorAsync();

            if (!errorBool && song.LocalPath != null)
            {
                Directory.Delete(song.LocalPath, true);
                Copy(versionPath + song.LocalPath, song.LocalPath);
            }

            return errorMessage;
        }

        public async Task<List<SongVersion>> versionsAsync(Song song)
        {
            return await Task.Run(() =>
            {
                return versions;
            });
        }

        public async Task<SongVersion> currentVersionAsync(Song song)
        {
            return await Task.Run(() =>
            {
                return currentVersion;
            });
        }

        private async Task<(bool, string)> UserErrorAsync()
        {
            return await Task.Run(() =>
            {
                if (userIsDifferentFrom(user1) &&
                    userIsDifferentFrom(user2))
                    return (true, "Error Bad Credentials");
                else
                    return (false, string.Empty);
            });
        }

        private bool userIsDifferentFrom(User expectedUser)
        {
            if (user == null ||
                user.BandName != expectedUser.BandName ||
                user.BandPassword != expectedUser.BandPassword ||
                user.Username != expectedUser.Username ||
                user.BandEmail != expectedUser.BandEmail)
                return true;
            return false;
        }

        void Copy(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)));

        }

        public async Task<string> downloadSharedSongAsync(string sharedLink, string downloadLocalPath)
        {
            return await Task.Run(() =>
            {
                return string.Empty;
            });
        }

        public async Task<string> shareSongAsync(Song song)
        {
            return await Task.Run(() =>
            {
                return "https://www.gitlab.com/test.git";
            });
        }

        public User user { get; set; }
        public string versionPath;

        private readonly User user1;
        private readonly User user2;
        private readonly SongVersion currentVersion;
        private readonly List<SongVersion> versions;
    }
}
