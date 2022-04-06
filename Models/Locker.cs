using App1.Models.Ports;

namespace App1.Models
{
    public class Locker
    {
        public Locker(IVersionTool NewVersionTool)
        {
            VersionTool = NewVersionTool;
        }

        public async Task<bool> lockSongAsync(Song song, User user)
        {
            if (!lockFileExist(song) || isLockedByUser(song, user))
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
                    removeLockFile(song);
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
                return File.ReadAllText(song.LocalPath + @"\.lock");
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
            File.WriteAllText(song.LocalPath + @"\.lock", user.Username);
        }

        private void deleteLockFile(Song song)
        {
            File.Delete(song.LocalPath + @"\.lock");
        }

        private void removeLockFile(Song song)
        {
            File.Delete(song.LocalPath + @"\.lock");
        }

        private bool lockFileExist(Song song)
        {
            if (File.Exists(song.LocalPath + @"\.lock"))
            {
                return true;
            }
            return false;
        }

        private readonly IVersionTool VersionTool;
    }
}
