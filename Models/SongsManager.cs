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
                updateSong(song);
        }

        public void updateSong(Song song)
        {
            versionTool_.pullChangesFromRepo(song.localPath);
        }

        public void uploadNewSongVersion(Song song, string changeTitle, string changeDescription)
        {
            versionTool_.addAllChanges(song.localPath);
            versionTool_.commitChanges(song.localPath, changeTitle, changeDescription);
            versionTool_.pushChangesToRepo(song.localPath);
        }

        public void addSong(string songTitle, string songFile, string songLocalPath)
        {
            Song song = new Song(songTitle, songFile, songLocalPath);
            songsList_.addNewSong(song);
        }

        public void deleteSong(Song song)
        {
            songsList_.deleteSong(song);
        }

        public void openSong(Song song)
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(song.localPath + @"\" + song.file )
            {
                UseShellExecute = true
            };
            p.Start();
        }

        private VersionTool versionTool_;
        public SongsStorage songsList_ { get; private set; }
    }
}