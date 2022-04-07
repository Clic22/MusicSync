namespace App1.Models.Ports
{
    public interface ISongsManager
    {
        SongsStorage SongList { get; }

        public Task<string> updateSongAsync(Song song);
        public Task<string> uploadNewSongVersionAsync(Song song, string changeTitle, string changeDescription, bool compo, bool mix, bool mastering);
        public Task<string> addSharedSongAsync(string songTitle, string sharedLink, string downloadLocalPath);
        public Task addLocalSongAsync(string songTitle, string songFile, string songLocalPath);
        public Task deleteSongAsync(Song song);
        public Task<string> shareSongAsync(Song song);
        public Task<bool> openSongAsync(Song song);
        public Task<string> revertSongAsync(Song song);
        public Song findSong(string songTitle);
        public Task<SongVersion> currentVersionAsync(Song song);
        public Task<List<SongVersion>> versionsAsync(Song song);
    }
}
