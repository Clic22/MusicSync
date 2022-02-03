using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1
{
    public sealed class Saver
    {
        public Saver()
        {
           localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
           songContainer = localSettings.CreateContainer("songs", Windows.Storage.ApplicationDataCreateDisposition.Always);
           userContainer = localSettings.CreateContainer("user", Windows.Storage.ApplicationDataCreateDisposition.Always);
        }

        public void saveUser(User user)
        {
            userContainer.Values.Remove("gitLabUsername");
            userContainer.Values.Add("gitLabUsername", user.gitLabUsername);
            userContainer.Values.Remove("gitLabPassword");
            userContainer.Values.Add("gitLabPassword", user.gitLabPassword);
            userContainer.Values.Remove("gitUsername");
            userContainer.Values.Add("gitUsername", user.gitUsername);
            userContainer.Values.Remove("gitEmail");
            userContainer.Values.Add("gitEmail", user.gitEmail);
        }

        public User savedUser()
        {
            User user = new User();
            user.gitLabUsername = userContainer.Values["gitLabUsername"] as string;
            user.gitLabPassword = userContainer.Values["gitLabPassword"] as string;
            user.gitUsername = userContainer.Values["gitUsername"] as string;
            user.gitEmail = userContainer.Values["gitEmail"] as string;
            return user;
        }

        public void saveSong(Song song)
        {
            Windows.Storage.ApplicationDataCompositeValue composite = new Windows.Storage.ApplicationDataCompositeValue();
            composite["file"] = song.file;
            composite["localPath"] = song.localPath;
            songContainer.Values[song.title] = composite;
        }

        public void unsaveSong(Song song)
        {
            songContainer.Values.Remove(song.title);
        }

        public List<Song> savedSongs()
        {
            List<Song> savedSongs = new List<Song>();
            foreach (var item in songContainer.Values)
            {
                Windows.Storage.ApplicationDataCompositeValue composite = (Windows.Storage.ApplicationDataCompositeValue)item.Value;
                Song song = new Song(item.Key, composite["file"] as string, composite["localPath"] as string);
                savedSongs.Add(song);
            }
            return savedSongs;
        }

        private Windows.Storage.ApplicationDataContainer localSettings;
        private Windows.Storage.ApplicationDataContainer songContainer;
        private Windows.Storage.ApplicationDataContainer userContainer;

    } 
}
