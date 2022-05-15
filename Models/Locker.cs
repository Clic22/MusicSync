using App1.Models.Ports;

namespace App1.Models
{
    public class Locker
    {
        public Locker(ISaver Saver, IFileManager FileManager, Versioning version)
        {
            this.version = version;
            this.FileManager = FileManager;
            workspace = new MusicSyncWorkspace(Saver, FileManager);
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
                try
                {
                    await version.uploadFileForSongAsync(song, @".lock", "lock");
                }
                catch
                {
                    deleteLockFile(song);
                    throw;
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
                    await version.uploadFileForSongAsync(song, @".lock", "unlock");
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

        private readonly IFileManager FileManager;
        private readonly Versioning version;
    }
}
