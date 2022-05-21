using App1.Models;
using App1.Models.Ports;

namespace App1Tests.Mock
{
    public class SaverMock : ISaver
    {
        public SaverMock()
        {
            _songs = new List<Song>();
            _user = new User();
            _musicSyncFolder = string.Empty;
        }

        public User SavedUser()
        {
            return _user;
        }

        public void SaveSong(Song song)
        {
            _songs.Add(song);
        }

        public void UnsaveSong(Song song)
        {
            _songs.Remove(song);
        }

        public List<Song> SavedSongs()
        {
            return _songs;
        }

        public void SaveSettings(User user, string musicSyncFolder)
        {
            SaveUser(user);
            _musicSyncFolder = musicSyncFolder;
        }

        public string SavedMusicSyncFolder()
        {
            return _musicSyncFolder;
        }

        private void SaveUser(User user)
        {
            _user = user;
        }

        private readonly List<Song> _songs;
        private User _user;
        private string _musicSyncFolder;

    }
}
