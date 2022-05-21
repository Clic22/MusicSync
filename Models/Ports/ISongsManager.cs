namespace App1.Models.Ports
{
    public interface ISongsManager
    {
        SongsStorage SongList { get; }

        public Task UpdateSongAsync(Song song);
        public Task UploadNewSongVersionAsync(Song song, string changeTitle, string changeDescription, bool compo, bool mix, bool mastering);
        public Task AddSharedSongAsync(string songTitle, string sharedLink, string downloadLocalPath);
        public Task AddLocalSongAsync(string songTitle, string songFile, string songLocalPath);
        public Task DeleteSongAsync(Song song);
        public string ShareSong(Song song);
        public Task OpenSongAsync(Song song);
        public Task RevertSongAsync(Song song);
        public Task RefreshSongStatusAsync(Song song);
        public Song FindSong(string songTitle);
        public void RenameSong(Song song, string newSongTitle);
        public Task<SongVersion> CurrentVersionAsync(Song song);
        public Task<List<SongVersion>> VersionsAsync(Song song);
        public Task<List<SongVersion>> UpcomingVersionsAsync(Song song);
    }
}
