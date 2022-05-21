namespace App1.Models.Ports
{
    public interface ISaver
    {
        public void saveSettings(User user, string musicSyncFolder);
        public void SaveSong(Song song);
        public void unsaveSong(Song song);
        public User SavedUser();
        public List<Song> savedSongs();
        public string savedMusicSyncFolder();
    }
}
