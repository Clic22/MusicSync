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
                string errorMessage = await VersionTool.uploadSongAsync(song, "lock", string.Empty);
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
            if(isLockedByUser(song,user))
            {
                removeLockFile(song);
                await VersionTool.uploadSongAsync(song, "unlock", string.Empty);
                updateSongStatus(song);
                return true;
            }
            return false;
        }

        public bool isLockedByUser(Song song, User user)
        {
            if (lockFileExist(song))
            {
                if (lockFileCreatedByUser(song, user))
                    return true;
            }
            return false;

        }

        public void updateSongStatus(Song song)
        {
            if (lockFileExist(song))
            {
                song.Status = Song.SongStatus.locked;
            }
            else
            {
                song.Status = Song.SongStatus.upToDate;
            }
        }

        private bool lockFileCreatedByUser(Song song, User user)
        {
            string username = File.ReadAllText(song.LocalPath + @"\.lock");
            if (username == user.GitUsername)
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

        private IVersionTool VersionTool;
    }
}
