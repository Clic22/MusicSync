using App1.Models.Ports;
using System.Collections.ObjectModel;

namespace App1.Models
{
    public class SongsStorage : List<Song>
    {
        public SongsStorage(ISaver NewSaver)
        {
            Saver = NewSaver;
            foreach (Song song in Saver.savedSongs())
            {
                this.Add(song);
            }
        }

        public void addNewSong(Song newSong)
        {
            this.Add(newSong);
            Saver.saveSong(newSong);

        }

        public void deleteSong(Song song)
        {
            this.Remove(song);
            Saver.unsaveSong(song);
        }

        private readonly ISaver Saver;
    }
}
