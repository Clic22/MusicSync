using System.Diagnostics;

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
            {
                updateSong(song);
            }
        }

        public void updateSong(Song song)
        {
            versionTool_.pullChangesFromRepo(song);
            versionTool_.updateSongStatus(song);
        }

        public void uploadNewSongVersion(Song song, string changeTitle, string changeDescription)
        {
            if (versionTool_.isLockedByUser(song))
            {
                versionTool_.unlockSong(song);
                versionTool_.addCommitAndPush(song, changeTitle, changeDescription);
            }
        }

        public void addSong(string songTitle, string songFile, string songLocalPath)
        {
            Song song = new Song(songTitle, songFile, songLocalPath);
            songsList_.addNewSong(song);
        }

        public void deleteSong(Song song)
        {
            if (versionTool_.isLockedByUser(song))
            {
                versionTool_.unlockSong(song);
            }
            songsList_.deleteSong(song);
        }

        public bool openSong(Song song)
        {
            updateSong(song);
            if (song.status == Song.SongStatus.upToDate)
            {
                versionTool_.lockSong(song);
            }
            if (versionTool_.isLockedByUser(song))
            {
                openSongWithDAW(song);
                return true;
            }
            return false;
        }

        public void revertSong(Song song)
        {
            updateSong(song);
            if (versionTool_.isLockedByUser(song))
            {
                versionTool_.revertChanges(song);
                versionTool_.unlockSong(song);
            }
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

        private VersionTool versionTool_;
        public SongsStorage songsList_ { get; private set; }
    }
}