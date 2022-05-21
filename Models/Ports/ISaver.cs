namespace App1.Models.Ports
{
    public interface ISaver
    {
        public void SaveSettings(User user, string musicSyncFolder);
        public void SaveSong(Song song);
        public void UnsaveSong(Song song);
        public User SavedUser();
        public List<Song> SavedSongs();
        public string SavedMusicSyncFolder();
    }
}
