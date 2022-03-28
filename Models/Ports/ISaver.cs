namespace App1.Models.Ports
{
    public interface ISaver
    {
        public void saveUser(User user);
        public User savedUser();
        public void saveSong(Song song);
        public void unsaveSong(Song song);
        public List<Song> savedSongs();
    }
}
