using App1.Adapters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1
{
    public class Locker
    {
        public Locker() 
        {
            versionTool = new GitSongVersioning();
        }

        public async void lockSong(Song song)
        {
            createLockFile(song);
            await versionTool.uploadSongAsync(song, "lock", string.Empty);
            updateSongStatus(song);
        }

        public async void unlockSong(Song song)
        {
            removeLockFile(song);
            await versionTool.uploadSongAsync(song, "unlock", string.Empty);
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
            Saver saver = new Saver();
            User user = saver.savedUser();
            string username = File.ReadAllText(song.localPath + @"\.lock");
            if (username == user.gitUsername)
            {
                return true;
            }
            return false;
        }

        private void createLockFile(Song song)
        {
            Saver saver = new Saver();
            User user = saver.savedUser();
            File.WriteAllText(song.localPath + @"\.lock", user.gitUsername);
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

        private IVersionTool versionTool;

    }
}
