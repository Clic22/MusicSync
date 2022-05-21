using App1.Models.Ports;
using System.Diagnostics;

namespace App1.Models
{
    public class SongsManager : ISongsManager
    {
        public SongsManager(ITransport transport, ISaver saver, IFileManager fileManager)
        {
            _saver = saver;
            SongList = new SongsStorage(_saver);
            _fileManager = fileManager;
            _versionTool = new Versioning(_saver, _fileManager, transport);
            _locker = new Locker(_fileManager, _versionTool);
        }

        public async Task UpdateSongAsync(Song song)
        {
            if (await _versionTool.UpdatesAvailableForSongAsync(song))
            {
                await _versionTool.UpdateSongAsync(song);
            }
            await RefreshSongStatusAsync(song);
        }

        public async Task UploadNewSongVersionAsync(Song song, string changeTitle, string changeDescription, bool compo, bool mix, bool mastering)
        {
            if (await _locker.UnlockSongAsync(song, _saver.SavedUser()))
            {
                string versionNumber = await _versionTool.NewVersionNumberAsync(song, compo, mix, mastering);
                await _versionTool.UploadSongAsync(song, changeTitle, changeDescription, versionNumber);
            }
        }

        public async Task AddLocalSongAsync(string songTitle, string songFile, string songLocalPath)
        {
            string songGuid = Guid.NewGuid().ToString();
            Song song = AddSong(songTitle, songFile, songLocalPath, songGuid);
            await _versionTool.UploadSongAsync(song, "First Upload", String.Empty, "1.0.0");
        }

        public async Task AddSharedSongAsync(string songTitle, string sharedLink, string downloadLocalPath)
        {
            string songPath = _fileManager.FormatPath(downloadLocalPath + songTitle);
            await _versionTool.DownloadSharedSongAsync(sharedLink, songPath);
            string songGuid = _versionTool.GuidFromSharedLink(sharedLink);
            string? songFile = await _fileManager.FindFileNameBasedOnExtensionAsync(songPath, ".song");
            if (songFile != null)
            {
                AddSong(songTitle, songFile, songPath, songGuid);
            }
        }

        public void RenameSong(Song song, string newSongTitle)
        {
                string formerLocalPath = song.LocalPath;
                string newLocalPath = _fileManager.FormatPath(formerLocalPath.Replace(song.Title + '\\', "") + newSongTitle);
                _fileManager.RenameFolder(formerLocalPath, newLocalPath);
                song.LocalPath = newLocalPath;
                song.Title = newSongTitle;
                string formerFile = song.File;
                string newFile = newSongTitle + ".song";
                _fileManager.RenameFile(formerFile, newFile, song.LocalPath);
                song.File = newSongTitle + ".song";
                _saver.SaveSong(song);  
        }

        public async Task DeleteSongAsync(Song song)
        {
            await _locker.UnlockSongAsync(song, _saver.SavedUser());
            SongList.DeleteSong(song);
        }

        public async Task OpenSongAsync(Song song)
        {
            await UpdateSongAsync(song);

            bool lockedByUser = await _locker.LockSongAsync(song, _saver.SavedUser());
            if (lockedByUser)
            {
                OpenSongWithDAW(song);
            }
            else
            {
                throw new SongsManagerException("Song locked by " + song.Status.whoLocked);
            }
        }

        public async Task RevertSongAsync(Song song)
        {
            if (await _locker.UnlockSongAsync(song, _saver.SavedUser()))
            {
                await _versionTool.RevertSongAsync(song);
            }
        }

        public Song FindSong(string songTitle)
        {
            Song? song = SongList.Find(song => song.Title == songTitle);
            if (song != null)
            {
                return song;
            }
            else
            {
                throw new SongsManagerException("Song not Found with title : " + songTitle);
            }
        }

        public async Task<SongVersion> CurrentVersionAsync(Song song)
        {
            return await _versionTool.CurrentVersionAsync(song);
        }

        public async Task<List<SongVersion>> VersionsAsync(Song song)
        {
            return await _versionTool.VersionsAsync(song);
        }

        public async Task<List<SongVersion>> UpcomingVersionsAsync(Song song)
        {
            return await _versionTool.UpcomingVersionsAsync(song);
        }

        public string ShareSong(Song song)
        {
            return _versionTool.ShareSong(song);
        }

        public async Task RefreshSongStatusAsync(Song song)
        {
            if (await _versionTool.UpdatesAvailableForSongAsync(song))
            {
                song.Status.state = SongStatus.State.updatesAvailable;
            }
            else if (_locker.IsLocked(song))
            {
                song.Status.state = SongStatus.State.locked;
                song.Status.whoLocked = _locker.WhoLocked(song);
            }
            else
            {
                song.Status.state = SongStatus.State.upToDate;
            }
        }

        private Song AddSong(string songTitle, string songFile, string songLocalPath, string songGuid)
        {
            Song song = new Song(songTitle, songFile, songLocalPath, songGuid);
            SongList.AddNewSong(song);
            return song;
        }

        private static void OpenSongWithDAW(Song song)
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(song.LocalPath + @"\" + song.File)
            {
                UseShellExecute = true
            };
            p.Start();
        }

        public SongsStorage SongList { get; private set; }
        private readonly Versioning _versionTool;
        private readonly Locker _locker;
        private readonly ISaver _saver;
        private readonly IFileManager _fileManager;
    }
}
