using System;
using System.Diagnostics;
using System.IO;

namespace App1
{
    public class SongsManager
    {
        public SongsManager()
        {
            versionTool_ = new VersionTool(User.Instance);
            songsList_ = new SongsStorage();
        }

        public void updateAllSongs()
        {
            foreach (Song song in songsList_)
                updateSong(song);
        }

        public void updateSong(Song song)
        {
            versionTool_.pullChangesFromRepo(song);
            song.status = Song.SongStatus.upToDate;
        }

        public void uploadNewSongVersion(Song song, string changeTitle, string changeDescription)
        {
            unlockSong(song);
            versionTool_.addCommitAndPush(song, changeTitle, changeDescription);  
        }

        public void addSong(string songTitle, string songFile, string songLocalPath)
        {
            Song song = new Song(songTitle, songFile, songLocalPath);
            songsList_.addNewSong(song);
        }

        public void deleteSong(Song song)
        {
            if (song.status == Song.SongStatus.locked)
            {
                unlockSong(song);
            }
            songsList_.deleteSong(song);
        }

        public void openSong(Song song)
        {
            lockSong(song);
            openSongWithDAW(song);
        }

        public void revertSong(Song song)
        {
            versionTool_.revertChanges(song);
            unlockSong(song);
        }

        private static void openSongWithDAW(Song song)
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(song.localPath + @"\" + song.file)
            {
                UseShellExecute = true
            };
            p.Start();
        }

        private void lockSong(Song song)
        {
            createLockFile(song);
            uploadLock(song);
            song.status = Song.SongStatus.locked;
        }

        private void unlockSong(Song song)
        {
            removeLockFile(song);
            uploadUnlock(song);
            song.status = Song.SongStatus.upToDate;
        }

        private void createLockFile(Song song)
        {
            FileStream filestream = File.Create(song.localPath + @"\.lock");
            filestream.Close();
        }

        private void removeLockFile(Song song)
        {
            File.Delete(song.localPath + @"\.lock");
        }

        private void uploadLock(Song song)
        {
            versionTool_.addLockCommitAndPush(song, "lock");
        }

        private void uploadUnlock(Song song)
        {
            versionTool_.addLockCommitAndPush(song, "unlock");
        }

        private VersionTool versionTool_;
        public SongsStorage songsList_ { get; private set; }
    }
}