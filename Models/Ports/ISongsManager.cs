using System.Collections.Generic;

namespace App1.Models.Ports
{
    public interface ISongsManager
    {
        SongsStorage SongList { get; }

        public Task<string> updateAllSongsAsync();
        public Task<string> updateSongAsync(Song song);
        public Task<string> uploadNewSongVersion(Song song, string changeTitle, string changeDescription);
        public void addSong(string songTitle, string songFile, string songLocalPath);
        public Task deleteSong(Song song);
        public Task<(bool, string)> openSongAsync(Song song);
        public Task<string> revertSongAsync(Song song);
        public Song? findSong(string songTitle);
    }
}
