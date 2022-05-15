namespace App1.Models.Ports
{
    public interface IVersionTool
    {
        public Task uploadSongAsync(Song song, string title, string description, string versionNumber);
        public Task uploadFileForSongAsync(Song song, string file, string title);
        public Task downloadSharedSongAsync(string sharedLink, string songPath);
        public string shareSong(Song song);
        public Task updateSongAsync(Song song);
        public Task<bool> updatesAvailableForSongAsync(Song song);
        public Task revertSongAsync(Song song);
        public Task<SongVersion> currentVersionAsync(Song song);
        public Task<List<SongVersion>> versionsAsync(Song song);
        public Task<List<SongVersion>> upcomingVersionsAsync(Song song);
        public Task<string> newVersionNumberAsync(Song song, bool compo, bool mix, bool mastering);
    }
}