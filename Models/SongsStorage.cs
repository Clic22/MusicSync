using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace App1
{
    public class SongsStorage : ObservableCollection<Song>
    {
        public SongsStorage()
        {
            localSettings_ = Windows.Storage.ApplicationData.Current.LocalSettings;
            foreach (var item in localSettings_.Values)
            {   
                Song song = new Song();
                song.title = item.Key;
                song.localPath = item.Value as string;
                this.Add(song);
            }
        }

        public void addNewSong(Song newSong)
        {
            this.Add(newSong);
            localSettings_.Values[newSong.title] = newSong.localPath;
        }

        public void deleteSong(Song song)
        {
            this.Remove(song);
            localSettings_.Values.Remove(song.title);
        }

        private Windows.Storage.ApplicationDataContainer localSettings_;
    }   
}
