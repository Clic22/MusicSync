using App1.Models.Ports;

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

        public void AddNewSong(Song newSong)
        {
            this.Add(newSong);
            Saver.SaveSong(newSong);

        }

        public void DeleteSong(Song song)
        {
            this.Remove(song);
            Saver.unsaveSong(song);
        }

        private readonly ISaver Saver;
    }
}
