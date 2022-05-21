﻿using App1.Models.Ports;

namespace App1.Models
{
    public class Locker
    {
        public Locker(IFileManager fileManager, Versioning version)
        {
            _version = version;
            _fileManager = fileManager;
        }

        public async Task<bool> LockSongAsync(Song song, User user)
        {
            if(isLockedByUser(song,user))
            {
                return true;
            }
            else if (!LockFileExist(song))
            {
                CreateLockFile(song, user);
                try
                {
                    await _version.UploadFileForSongAsync(song, @".lock", "lock");
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

        public async Task<bool> UnlockSongAsync(Song song, User user)
        {
            if (LockFileExist(song))
            {
                if (isLockedByUser(song, user))
                {
                    deleteLockFile(song);
                    await _version.UploadFileForSongAsync(song, @".lock", "unlock");
                    return true;
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool IsLocked(Song song)
        {
            if (LockFileExist(song))
            {
                return true;
            }
            return false;
        }

        public bool isLockedByUser(Song song, User user)
        {
            if (LockFileExist(song) && songLockedByUser(song, user))
            {
                return true;
            }
            return false;
        }

        public string WhoLocked(Song song)
        {
            if (LockFileExist(song))
            {
                return _fileManager.ReadFile(@".lock", song.LocalPath);
            }
            return String.Empty;
        }

        private bool songLockedByUser(Song song, User user)
        {
            if (WhoLocked(song) == user.Username)
            {
                return true;
            }
            return false;
        }

        private void CreateLockFile(Song song, User user)
        {
            string fileName = @".lock";
            _fileManager.CreateFile(fileName, song.LocalPath);
            _fileManager.WriteFile(user.Username, fileName, song.LocalPath);
        }

        private void deleteLockFile(Song song)
        {
            _fileManager.DeleteFile(".lock", song.LocalPath);
        }

        private bool LockFileExist(Song song)
        {
            return _fileManager.FileExists(@".lock",song.LocalPath);
        }

        private readonly IFileManager _fileManager;
        private readonly Versioning _version;
    }
}
