using App1.Models;
using App1.Models.Ports;
using System.Collections.Generic;

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

        public void saveUser(User user)
        {
            UserContainer.Values.Remove("gitLabUsername");
            UserContainer.Values.Add("gitLabUsername", user.GitLabUsername);
            UserContainer.Values.Remove("gitLabPassword");
            UserContainer.Values.Add("gitLabPassword", user.GitLabPassword);
            UserContainer.Values.Remove("gitUsername");
            UserContainer.Values.Add("gitUsername", user.GitUsername);
            UserContainer.Values.Remove("gitEmail");
            UserContainer.Values.Add("gitEmail", user.GitEmail);
        }

        public User savedUser()
        {
            User user = new User();
            user.GitLabUsername = UserContainer.Values["gitLabUsername"] as string;
            user.GitLabPassword = UserContainer.Values["gitLabPassword"] as string;
            user.GitUsername = UserContainer.Values["gitUsername"] as string;
            user.GitEmail = UserContainer.Values["gitEmail"] as string;
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
            foreach (var item in SongContainer.Values)
            {
                Windows.Storage.ApplicationDataCompositeValue composite = (Windows.Storage.ApplicationDataCompositeValue)item.Value;
                Song song = new Song(item.Key, composite["file"] as string, composite["localPath"] as string);
                savedSongs.Add(song);
            }
            return savedSongs;
        }

        private readonly Windows.Storage.ApplicationDataContainer LocalSettings;
        private readonly Windows.Storage.ApplicationDataContainer SongContainer;
        private readonly Windows.Storage.ApplicationDataContainer UserContainer;

    }
}
