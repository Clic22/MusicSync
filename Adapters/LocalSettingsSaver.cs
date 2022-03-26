using App1.Models;
using App1.Models.Ports;

namespace App1.Adapters
{
    public class LocalSettingsSaver : ISaver
    {
        public LocalSettingsSaver()
        {
            Windows.Storage.ApplicationDataContainer LocalSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            SongContainer = LocalSettings.CreateContainer("songs", Windows.Storage.ApplicationDataCreateDisposition.Always);
            UserContainer = LocalSettings.CreateContainer("user", Windows.Storage.ApplicationDataCreateDisposition.Always);
        }

        public void saveUser(User user)
        {
            saveUserValue("BandName", user.BandName);
            saveUserValue("BandPassword", user.BandPassword);
            saveUserValue("Username", user.Username);
            saveUserValue("BandEmail", user.BandEmail);
        }

        public User savedUser()
        {
            User user = new User();
            user.BandName = UserContainer.Values["BandName"] as string;
            user.BandPassword = UserContainer.Values["BandPassword"] as string;
            user.Username = UserContainer.Values["Username"] as string;
            user.BandEmail = UserContainer.Values["BandEmail"] as string;
            return user;
        }

        public void saveSong(Song song)
        {
            Windows.Storage.ApplicationDataCompositeValue composite = new Windows.Storage.ApplicationDataCompositeValue();
            composite["file"] = song.File;
            composite["localPath"] = song.LocalPath;
            SongContainer.Values[song.Title] = composite;
        }

        public void unsaveSong(Song song)
        {
            SongContainer.Values.Remove(song.Title);
        }

        public List<Song> savedSongs()
        {
            List<Song> savedSongs = new List<Song>();
            string? file;
            string? localPath;

            foreach (var item in SongContainer.Values)
            {
                Windows.Storage.ApplicationDataCompositeValue composite = (Windows.Storage.ApplicationDataCompositeValue)item.Value;
                file = composite["file"] as string;
                localPath = composite["localPath"] as string;
                if (localPath != null && file != null)
                {
                    Song song = new Song(item.Key, file, localPath);
                    savedSongs.Add(song);
                }
            }
            return savedSongs;
        }

        private void saveUserValue(string valueName, string? value)
        {
            UserContainer.Values.Remove(valueName);
            UserContainer.Values.Add(valueName, value);
        }

        private readonly Windows.Storage.ApplicationDataContainer SongContainer;
        private readonly Windows.Storage.ApplicationDataContainer UserContainer;
    }
}
