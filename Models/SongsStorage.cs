using System.Collections.ObjectModel;

namespace App1
{
    public class SongsStorage : ObservableCollection<Song>
    {
        public SongsStorage()
        {
            saver = new Saver();
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

        private Saver saver;
    }
}
