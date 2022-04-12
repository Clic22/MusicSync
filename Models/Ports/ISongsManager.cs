namespace App1.Models.Ports
{
    public interface ISongsManager
    {
        SongsStorage SongList { get; }

        public Task updateSongAsync(Song song);
        public Task uploadNewSongVersionAsync(Song song, string changeTitle, string changeDescription, bool compo, bool mix, bool mastering);
        public Task addSharedSongAsync(string songTitle, string sharedLink, string downloadLocalPath);
        public void addLocalSong(string songTitle, string songFile, string songLocalPath);
        public Task deleteSongAsync(Song song);
        public Task<string> shareSongAsync(Song song);
        public Task openSongAsync(Song song);
        public Task revertSongAsync(Song song);
        public Task refreshSongStatusAsync(Song song);
        public Song findSong(string songTitle);
        public Task<SongVersion> currentVersionAsync(Song song);
        public Task<List<SongVersion>> versionsAsync(Song song);
    }
}
