namespace App1.Models.Ports
{
    public interface ITransport
    {
        void Init(string songMusicSyncPath, string name);
        Task InitAsync(string songMusicSyncPath, string sharedLink);
        Task UploadAllFilesAsync(string songMusicSyncPath, string title, string description);
        Task UploadFileAsync(string songMusicSyncPath, string file, string title);
        void Tag(string songMusicSyncPath, string versionNumber);
        Task<bool> UpdatesAvailbleAsync(string songMusicSyncPath);
        Task DownloadLastUpdateAsync(string songMusicSyncPath);
        Task RevertToLastLocalVersionAsync(string songMusicSyncPath);
        Task<SongVersion>? LastLocalVersionAsync(string songMusicSyncPath);
        bool Initiated(string songMusicSyncPath);
        Task<List<SongVersion>>? LocalVersionsAsync(string songMusicSyncPath);
        Task<List<SongVersion>>? UpcomingVersionsAsync(string songMusicSyncPath);
        string ShareLink(string songMusicSyncPath);
        string GuidFromSharedLink(string sharedLink);
    }
}