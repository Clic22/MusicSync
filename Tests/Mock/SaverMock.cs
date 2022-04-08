using App1.Models;
using App1.Models.Ports;

namespace App1Tests.Mock
{
    public class SaverMock : ISaver
    {
        public SaverMock()
        {
            Songs = new List<Song>();
            User = new User();
            MusicSyncFolder = string.Empty;
        }

        public User savedUser()
        {
            return User;
        }

        public void saveSong(Song song)
        {
            Songs.Add(song);
        }

        public void unsaveSong(Song song)
        {
            Songs.Remove(song);
        }

        public List<Song> savedSongs()
        {
            return Songs;
        }

        public void saveSettings(Settings settings)
        {
            saveUser(settings.User);
            MusicSyncFolder = settings.MusicSyncFolder;
        }

        public string savedMusicSyncFolder()
        {
            return MusicSyncFolder;
        }

        private void saveUser(User user)
        {
            User = user;
        }

        public int savedCheckUpdatesFrequency()
        {
            throw new NotImplementedException();
        }

        private readonly List<Song> Songs;
        private User User;
        private string MusicSyncFolder;

    }
}
