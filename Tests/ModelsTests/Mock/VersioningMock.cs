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
            user1 = new User(GitLabUsername1, GitLabPassword1, GitUsername1, GitEmail1); ;
            string GitUsername2 = "Lithorama52";
            string GitLabPassword2 = "15@^_usnjdfb@";
            string GitLabUsername2 = "Erratum12";
            string GitEmail2 = "erratum12@gmail.com";
            user2 = new User(GitLabUsername2, GitLabPassword2, GitUsername2, GitEmail2);
            versionPath = @"./versionStorage/";
            Directory.CreateDirectory(versionPath);
            songVersionDescription = string.Empty;
        }

        public async Task<string> uploadSongAsync(Song song, string title, string description, string versionNumber)
        {
            (bool errorBool, string errorMessage) = await UserErrorAsync();
            songVersionDescription = title + "\n\n" + description;
            if (!errorBool)
            {
                if (song.LocalPath != null)
                {
                    if(Directory.Exists(versionPath + song.LocalPath))
                    {
                        Directory.Delete(versionPath + song.LocalPath, true);
                    }
                    Copy(song.LocalPath, versionPath + song.LocalPath);
                    songVersionNumber = versionNumber;
                }
            }
            return errorMessage;
        }

        public async Task<string> uploadSongAsync(Song song, string file, string title)
        {
            (bool errorBool, string errorMessage) = await UserErrorAsync();
            if (!errorBool)
            {
                if (song.LocalPath != null)
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
            }
            return errorMessage;
        }

        public async Task<string> updateSongAsync(Song song)
        {
            (bool errorBool, string errorMessage) = await UserErrorAsync();
            if (!errorBool)
            {
                if (song.LocalPath != null)
                {
                    Copy(versionPath + song.LocalPath, song.LocalPath);
                }
            }
            return errorMessage;
        }

        public async Task<string> revertSongAsync(Song song)
        {
            (bool errorBool, string errorMessage) = await UserErrorAsync();
            if (!errorBool)
            {
                if (song.LocalPath != null)
                {
                    Directory.Delete(song.LocalPath, true);
                    Copy(versionPath + song.LocalPath, song.LocalPath);
                }

            }
            return errorMessage;
        }

        public async Task<string> songVersionDescriptionAsync(Song song)
        {
            return songVersionDescription;
        }

        public async Task<string> songVersionNumberAsync(Song song)
        {
            return songVersionNumber;
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
        private User user1;
        private User user2;
        private string songVersionDescription;
        private string songVersionNumber;
    }
}
