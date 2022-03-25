using App1.Models.Ports;
using System.IO;
using System.Threading.Tasks;

namespace App1.Models
{
    public class Locker
    {
        public Locker(IVersionTool NewVersionTool)
        {
            VersionTool = NewVersionTool;
        }

        public async Task<(bool,string)> lockSongAsync(Song song, User user)
        {
            if (!lockFileExist(song) || isLockedByUser(song,user))
            {
                createLockFile(song, user);
                string errorMessage = await VersionTool.uploadSongAsync(song, @".lock", "lock");
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    return (false,errorMessage);
                }
                updateSongStatus(song);
                return (true, "Song Locked");
            }
            return (false, "Already Locked");
        }

        public async Task<bool> unlockSongAsync(Song song, User user)
        {
            if(lockFileExist(song))
            {
                if(isLockedByUser(song,user))
                {
                    removeLockFile(song);
                    await VersionTool.uploadSongAsync(song, @".lock", "unlock");
                    updateSongStatus(song);
                    return true;
                }
            }
            else
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

        public void updateSongStatus(Song song)
        {
            if (lockFileExist(song))
            {
                song.Status.state = SongStatus.State.locked;
                string username = File.ReadAllText(song.LocalPath + @"\.lock");
                song.Status.whoLocked = username;
            }
            else
            {
                song.Status.state = SongStatus.State.upToDate;
            }
        }

        private bool songLockedByUser(Song song, User user)
        {
            if (song.Status.whoLocked == user.GitUsername)
            {
                return true;
            }
            return false;
        }

        private void createLockFile(Song song, User user)
        {
            File.WriteAllText(song.LocalPath + @"\.lock", user.GitUsername);
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
