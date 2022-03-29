using App1.Models;
using App1.Models.Ports;

namespace App1.Adapters
{
    public class LocalSettingsSaver : ISaver
    {
        public LocalSettingsSaver()
        {
            LocalSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            SongContainer = LocalSettings.CreateContainer("songs", Windows.Storage.ApplicationDataCreateDisposition.Always);
            UserContainer = LocalSettings.CreateContainer("user", Windows.Storage.ApplicationDataCreateDisposition.Always);
        }

        public void saveSettings(User user, string musicSyncFolder)
        {
            saveUserValue("BandName", user.BandName);
            saveUserValue("BandPassword", user.BandPassword);
            saveUserValue("Username", user.Username);
            saveUserValue("BandEmail", user.BandEmail);

            saveMusicSyncFolder(musicSyncFolder);
        }

        public User savedUser()
        {
            User user = new User();
            string? BandName = LocalSettings.Values["BandName"] as string;
            if (BandName != null)
            {
                user.BandName = BandName;
            }
            else
            {
                user.BandName = string.Empty;
            }
            string? BandPassword = LocalSettings.Values["BandPassword"] as string;
            if (BandPassword != null)
            {
                user.BandPassword = BandPassword;
            }
            else
            {
                user.BandPassword = string.Empty;
            }
            string? Username = LocalSettings.Values["Username"] as string;
            if (Username != null)
            {
                user.Username = Username;
            }
            else
            {
                user.Username = string.Empty;
            }
            string? BandEmail = LocalSettings.Values["BandEmail"] as string;
            if (BandEmail != null)
            {
                user.BandEmail = BandEmail;
            }
            else
            {
                user.BandEmail = string.Empty;
            }
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

        public string savedMusicSyncFolder()
        {
            string? MusicSyncFolder = LocalSettings.Values["MusicSyncFolder"] as string;
            if (MusicSyncFolder != null)
            {
                return MusicSyncFolder;
            }
            else
            {
                return string.Empty;
            }
            
        }

        private void saveUserValue(string valueName, string? value)
        {
            UserContainer.Values.Remove(valueName);
            UserContainer.Values.Add(valueName, value);
        }

        private void saveMusicSyncFolder(string musicSyncFolder)
        {
            LocalSettings.Values.Remove("MusicSyncFolder");
            LocalSettings.Values.Add("MusicSyncFolder", musicSyncFolder);
        }

        private readonly Windows.Storage.ApplicationDataContainer LocalSettings;
        private readonly Windows.Storage.ApplicationDataContainer SongContainer;
        private readonly Windows.Storage.ApplicationDataContainer UserContainer;
    }
}
