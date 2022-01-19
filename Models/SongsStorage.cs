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
            container_ = localSettings_.CreateContainer("songs", Windows.Storage.ApplicationDataCreateDisposition.Always);
            foreach (var item in container_.Values)
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
            container_.Values[newSong.title] = newSong.localPath;
        }

        public void deleteSong(Song song)
        {
            this.Remove(song);
            container_.Values.Remove(song.title);
        }

        private Windows.Storage.ApplicationDataContainer localSettings_;
        Windows.Storage.ApplicationDataContainer container_;
    }   
}
