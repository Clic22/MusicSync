namespace App1
{
    public class SongsManager
    {
        public SongsManager(SongsStorage songsList)
        {
            versionTool_ = new VersionTool();
            songsList_ = songsList;
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

        public void addSong(string songTitle, string songLocalPath)
        {
            Song song = new Song(songTitle,songLocalPath);
            songsList_.addNewSong(song);
        }

        public void deleteSong(Song song)
        {
            songsList_.deleteSong(song);
        }

        private VersionTool versionTool_;
        public SongsStorage songsList_ { get;}
    }
}