namespace App1.Models.Ports
{
    public interface ISaver
    {
        public void saveSettings(Settings settings);
        public void saveSong(Song song);
        public void unsaveSong(Song song);
        public User savedUser();
        public List<Song> savedSongs();
        public string savedMusicSyncFolder();
        public int savedCheckUpdatesFrequency();
    }
}
