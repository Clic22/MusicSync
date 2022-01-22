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
                
                Windows.Storage.ApplicationDataCompositeValue composite = (Windows.Storage.ApplicationDataCompositeValue) item.Value;
                Song song = new Song(item.Key,composite["file"] as string, composite["localPath"] as string);
                this.Add(song);
            }
        }

        public void addNewSong(Song newSong)
        {
            this.Add(newSong);
            Windows.Storage.ApplicationDataCompositeValue composite = new Windows.Storage.ApplicationDataCompositeValue();
            composite["file"] = newSong.file;
            composite["localPath"] = newSong.localPath;
            container_.Values[newSong.title] = composite;

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
