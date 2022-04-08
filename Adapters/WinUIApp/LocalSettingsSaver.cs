﻿using App1.Models;
using App1.Models.Ports;

namespace WinUIApp
{
    public class LocalSettingsSaver : ISaver
    {
        public LocalSettingsSaver()
        {
            LocalSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            SongContainer = LocalSettings.CreateContainer("songs", Windows.Storage.ApplicationDataCreateDisposition.Always);
            UserContainer = LocalSettings.CreateContainer("user", Windows.Storage.ApplicationDataCreateDisposition.Always);
        }

        public void saveSettings(Settings settings)
        {
            saveUserValue("BandName", settings.User.BandName);
            saveUserValue("BandPassword", settings.User.BandPassword);
            saveUserValue("Username", settings.User.Username);
            saveUserValue("BandEmail", settings.User.BandEmail);

            saveMusicSyncFolder(settings.MusicSyncFolder);

            saveCheckUpdatesFrequency(settings.CheckUpdatesFrequency);
        }

        public User savedUser()
        {
            User user = new User();
            string? BandName = UserContainer.Values["BandName"] as string;
            if (BandName != null)
            {
                user.BandName = BandName;
            }
            else
            {
                user.BandName = string.Empty;
            }
            string? BandPassword = UserContainer.Values["BandPassword"] as string;
            if (BandPassword != null)
            {
                user.BandPassword = BandPassword;
            }
            else
            {
                user.BandPassword = string.Empty;
            }
            string? Username = UserContainer.Values["Username"] as string;
            if (Username != null)
            {
                user.Username = Username;
            }
            else
            {
                user.Username = string.Empty;
            }
            string? BandEmail = UserContainer.Values["BandEmail"] as string;
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

        public int savedCheckUpdatesFrequency()
        {
            int? CheckUpdatesFrequency = LocalSettings.Values["CheckUpdatesFrequency"] as int?;
            if (CheckUpdatesFrequency != null)
            {
                return (int)CheckUpdatesFrequency;
            }
            else
            {
                return 0;
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

        private void saveCheckUpdatesFrequency(int checkUpdateFrequency)
        {
            LocalSettings.Values.Remove("CheckUpdatesFrequency");
            LocalSettings.Values.Add("CheckUpdatesFrequency", checkUpdateFrequency);
        }

        private readonly Windows.Storage.ApplicationDataContainer LocalSettings;
        private readonly Windows.Storage.ApplicationDataContainer SongContainer;
        private readonly Windows.Storage.ApplicationDataContainer UserContainer;
    }
}
