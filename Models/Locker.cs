using App1.Models.Ports;
using System.IO;

namespace App1.Models
{
    public class Locker
    {
        public Locker(IVersionTool NewVersionTool, User NewUser)
        {
            VersionTool = NewVersionTool;
            User = NewUser;
        }

        public async void lockSong(Song song)
        {
            createLockFile(song);
            await VersionTool.uploadSongAsync(song, "lock", string.Empty);
            updateSongStatus(song);
        }

        public async void unlockSong(Song song)
        {
            removeLockFile(song);
            await VersionTool.uploadSongAsync(song, "unlock", string.Empty);
            updateSongStatus(song);
        }

        public bool isLockedByUser(Song song)
        {
            if (lockFileExist(song))
            {
                if (lockFileCreatedByUser(song))
                    return true;
            }
            return false;

        }

        public void updateSongStatus(Song song)
        {
            if (lockFileExist(song))
            {
                song.status = Song.SongStatus.locked;
            }
            else
            {
                song.status = Song.SongStatus.upToDate;
            }
        }

        private bool lockFileCreatedByUser(Song song)
        {
            string username = File.ReadAllText(song.localPath + @"\.lock");
            if (username == User.gitUsername)
            {
                return true;
            }
            return false;
        }

        private void createLockFile(Song song)
        {
            File.WriteAllText(song.localPath + @"\.lock", User.gitUsername);
        }

        private void removeLockFile(Song song)
        {
            File.Delete(song.localPath + @"\.lock");
        }

        private bool lockFileExist(Song song)
        {
            if (File.Exists(song.localPath + @"\.lock"))
            {
                return true;
            }
            return false;
        }

        private IVersionTool VersionTool;
        private User User;

    }
}
