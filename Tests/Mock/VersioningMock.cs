using App1.Models;
using App1.Models.Ports;
using System.IO;
using System.Threading.Tasks;


namespace App1Tests.Mock
{
    public class VersioningMock : IVersionTool
    {
        public VersioningMock(User user) {
            this.user = user;
            string GitUsername1 = "Hear@fdjskjè_";
            string GitLabPassword1 = "12df546@";
            string GitLabUsername1 = "Clic5456";
            string GitEmail1 = "testdklsjfhg@yahoo.com";
            user1 = new User(GitLabUsername1, GitLabPassword1, GitUsername1, GitEmail1);
            string GitUsername2 = "Lithorama52";
            string GitLabPassword2 = "15@^_usnjdfb@";
            string GitLabUsername2 = "Erratum12";
            string GitEmail2 = "erratum12@gmail.com";
            user2 = new User(GitLabUsername2, GitLabPassword2, GitUsername2, GitEmail2);
            versionPath = @"./versionStorage/";
            Directory.CreateDirectory(versionPath);
            versionDescription = new Dictionary<Song, string>();
            versionNumber = new Dictionary<Song, string>();
            versions = new List<(string,string)>();
        }

        public async Task<string> uploadSongAsync(Song song, string title, string description, string versionNumber)
        {
            (bool errorBool, string errorMessage) = await UserErrorAsync();

            if (!errorBool && song.LocalPath != null)
            {
                if(Directory.Exists(versionPath + song.LocalPath))
                {
                    Directory.Delete(versionPath + song.LocalPath, true);
                }
                Copy(song.LocalPath, versionPath + song.LocalPath);
                versionDescription[song] = title + "\n\n" + description;
                this.versionNumber[song] = versionNumber;
                versions.Add((this.versionNumber[song], versionDescription[song]));
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
                versionDescription[song] = title;
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

        public async Task<string> versionDescriptionAsync(Song song)
        {
            return await Task.Run(() =>
            {
                return versionDescription[song];
            });
        }

        public async Task<string> versionNumberAsync(Song song)
        {
            return await Task.Run(() =>
            {
                try
                {
                    return versionNumber[song];
                }
                catch (Exception)
                {
                    return string.Empty;
                }
                
            });
        }

        public async Task<List<(string, string)>> versionsAsync(Song song)
        {
            return await Task.Run(() =>
            {
                return versions;
            });
        }

        private async Task<(bool,string)> UserErrorAsync()
        {
            return await Task.Run(() =>
            {
                if (userIsDifferentFrom(user1) && 
                    userIsDifferentFrom(user2))
                    return (true,"Error Bad Credentials");
                else
                    return (false,string.Empty);
            });
        }
        
        private bool userIsDifferentFrom(User expectedUser)
        {
            if (user == null || 
                user.GitLabUsername != expectedUser.GitLabUsername ||
                user.GitLabPassword != expectedUser.GitLabPassword ||
                user.GitUsername != expectedUser.GitUsername ||
                user.GitEmail != expectedUser.GitEmail)
                return true;
            return false;
        }

        void Copy(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)));

        }



        public User user { get; set; }
        public string versionPath;

        private readonly User user1;
        private readonly User user2;
        private readonly Dictionary<Song, string> versionDescription;
        private readonly Dictionary<Song, string> versionNumber;
        private readonly List<(string, string)> versions;
    }
}
