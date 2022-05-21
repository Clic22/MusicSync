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

        public User SavedUser()
        {
            return User;
        }

        public void SaveSong(Song song)
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

        public void saveSettings(User user, string musicSyncFolder)
        {
            saveUser(user);
            MusicSyncFolder = musicSyncFolder;
        }

        public string savedMusicSyncFolder()
        {
            return MusicSyncFolder;
        }

        private void saveUser(User user)
        {
            User = user;
        }

        private readonly List<Song> Songs;
        private User User;
        private string MusicSyncFolder;

    }
}
