namespace App1.Models.Ports
{
    public interface ITransport
    {
        void init(string songMusicSyncPath, string name);
        Task initAsync(string songMusicSyncPath, string sharedLink);
        Task uploadAllFilesAsync(string songMusicSyncPath, string title, string description);
        Task uploadFileAsync(string songMusicSyncPath, string file, string title);
        void tag(string songMusicSyncPath, string versionNumber);
        Task<bool> updatesAvailbleAsync(string songMusicSyncPath);
        Task downloadLastUpdateAsync(string songMusicSyncPath);
        Task revertToLastLocalVersionAsync(string songMusicSyncPath);
        Task<SongVersion>? lastLocalVersionAsync(string songMusicSyncPath);
        bool initiated(string songMusicSyncPath);
        Task<List<SongVersion>>? localVersionsAsync(string songMusicSyncPath);
        Task<List<SongVersion>>? upcomingVersionsAsync(string songMusicSyncPath);
        string shareLink(string songMusicSyncPath);
    }
}