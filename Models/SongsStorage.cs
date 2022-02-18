using App1.Models.Ports;
using System.Collections.ObjectModel;

namespace App1.Models
{
    public class SongsStorage : ObservableCollection<Song>
    {
        public SongsStorage(ISaver NewSaver)
        {
            saver = NewSaver;
            foreach (Song song in saver.savedSongs())
            {
                this.Add(song);
            }
        }

        public void addNewSong(Song newSong)
        {
            this.Add(newSong);
            saver.saveSong(newSong);

        }

        public void deleteSong(Song song)
        {
            this.Remove(song);
            saver.unsaveSong(song);
        }

        private ISaver saver;
    }
}
