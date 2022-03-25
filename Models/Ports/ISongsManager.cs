using System.Collections.Generic;

namespace App1.Models.Ports
{
    public interface ISongsManager
    {
        SongsStorage SongList { get; }

        public Task<string> updateAllSongsAsync();
        public Task<string> updateSongAsync(Song song);
        public Task<string> uploadNewSongVersionAsync(Song song, string changeTitle, string changeDescription, bool compo, bool mix, bool mastering);
        public void addSong(string songTitle, string songFile, string songLocalPath);
        public Task deleteSongAsync(Song song);
        public Task<(bool, string)> openSongAsync(Song song);
        public Task<string> revertSongAsync(Song song);
        public Song findSong(string songTitle);
        public Task<string> versionDescriptionAsync(Song song);
        public Task<string> versionNumberAsync(Song song);
        public Task<List<(string,string)>> versionsAsync(Song song);
    }
}
