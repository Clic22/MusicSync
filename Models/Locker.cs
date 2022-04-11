using App1.Models.Ports;

namespace App1.Models
{
    public class Locker
    {
        public Locker(IVersionTool NewVersionTool, IFileManager NewFileManager)
        {
            VersionTool = NewVersionTool;
            FileManager = NewFileManager;
        }

        public async Task<bool> lockSongAsync(Song song, User user)
        {
            if(isLockedByUser(song,user))
            {
                return true;
            }
            else if (!lockFileExist(song))
            {
                createLockFile(song, user);
                string errorMessage = await VersionTool.uploadSongAsync(song, @".lock", "lock");
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    deleteLockFile(song);
                    return false;
                }
                return true;
            }
            return false;
        }

        public async Task<bool> unlockSongAsync(Song song, User user)
        {
            if (lockFileExist(song))
            {
                if (isLockedByUser(song, user))
                {
                    deleteLockFile(song);
                    string errorMessage = await VersionTool.uploadSongAsync(song, @".lock", "unlock");
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool isLocked(Song song)
        {
            if (lockFileExist(song))
            {
                return true;
            }
            return false;
        }

        public bool isLockedByUser(Song song, User user)
        {
            if (lockFileExist(song) && songLockedByUser(song, user))
            {
                return true;
            }
            return false;
        }

        public string whoLocked(Song song)
        {
            if (lockFileExist(song))
            {
                return FileManager.ReadFile(@".lock", song.LocalPath);
            }
            return String.Empty;
        }

        private bool songLockedByUser(Song song, User user)
        {
            if (whoLocked(song) == user.Username)
            {
                return true;
            }
            return false;
        }

        private void createLockFile(Song song, User user)
        {
            string fileName = @".lock";
            FileManager.CreateFile(fileName, song.LocalPath);
            FileManager.WriteFile(user.Username, fileName, song.LocalPath);
        }

        private void deleteLockFile(Song song)
        {
            FileManager.DeleteFile(".lock", song.LocalPath);
        }

        private bool lockFileExist(Song song)
        {
            return FileManager.FileExists(@".lock",song.LocalPath);
        }

        private readonly IVersionTool VersionTool;
        private readonly IFileManager FileManager;
    }
}
