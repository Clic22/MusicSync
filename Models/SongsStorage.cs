using App1.Models.Ports;

namespace App1.Models
{
    public class SongsStorage : List<Song>
    {
        public SongsStorage(ISaver saver)
        {
            _saver = saver;
            foreach (Song song in _saver.SavedSongs())
            {
                this.Add(song);
            }
        }

        public void AddNewSong(Song newSong)
        {
            this.Add(newSong);
            _saver.SaveSong(newSong);

        }

        public void DeleteSong(Song song)
        {
            this.Remove(song);
            _saver.UnsaveSong(song);
        }

        private readonly ISaver _saver;
    }
}
