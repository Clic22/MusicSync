using App1.Models;
using App1.Models.Ports;
using System.Collections.ObjectModel;
using System.Globalization;
using WinUIApp;

namespace App1.ViewModels
{
    public class SongsPageViewModel : Bindable
    {
        public SongsPageViewModel(ISongsManager songsManager)
        {
            SongsVersioned = new ObservableCollection<SongVersioned>();
            _songsManager = songsManager;
            _fileManager = new FileManager();
            IntializeSongsVersioned();
        }

        public async Task<SongVersioned> AddLocalSongAsync(string songTitle, string songFile, string songLocalPath)
        {
            try
            {
                IsAddingSong = true;
                songLocalPath = _fileManager.FormatPath(songLocalPath);
                await _songsManager.AddLocalSongAsync(songTitle, songFile, songLocalPath);
                SongVersioned songVersioned = new SongVersioned(songTitle);
                SongsVersioned.Add(songVersioned);
                Song song = _songsManager.FindSong(songVersioned.Title);
                await RefreshSongVersionedAsync(songVersioned, song);
                IsAddingSong = false;
                return songVersioned;
            }
            catch
            {
                IsAddingSong = false;
                throw;
            }
            
        }

        public async Task AddSharedSongAsync(string songTitle, string sharedLink, string songLocalPath)
        {
            try
            {
                IsAddingSong = true;
                songLocalPath = _fileManager.FormatPath(songLocalPath);
                await _songsManager.AddSharedSongAsync(songTitle, sharedLink, songLocalPath);
                SongVersioned songVersioned = new SongVersioned(songTitle);
                SongsVersioned.Add(songVersioned);
                await UpdateSongAsync(songVersioned);
                IsAddingSong = false;
            }
            catch
            {
                IsAddingSong = false;
                throw;
            }
        }

        public async Task DeleteSongAsync(SongVersioned songVersioned)
        {
            try
            {
                Song song = _songsManager.FindSong(songVersioned.Title);
                await _songsManager.DeleteSongAsync(song);
                SongsVersioned.Remove(songVersioned);
            }
            catch
            {
                songVersioned.Status = "Error";
                throw;
            }
              
        }

        public async Task UpdateSongAsync(SongVersioned songVersioned)
        {
            try
            {
                songVersioned.IsUpdatingSong = true;
                Song song = _songsManager.FindSong(songVersioned.Title);
                await _songsManager.UpdateSongAsync(song);
                await RefreshSongVersionedAsync(songVersioned, song);
                songVersioned.IsUpdatingSong = false;
            }
            catch
            {
                songVersioned.IsUpdatingSong = false;
                songVersioned.Status = "Error";
                throw;
            }
        }

        public async Task OpenSongAsync(SongVersioned songVersioned)
        {
            try
            {
                songVersioned.IsOpeningSong = true;
                Song song = _songsManager.FindSong(songVersioned.Title);
                await _songsManager.OpenSongAsync(song);
                songVersioned.IsOpeningSong = false;
                await RefreshSongStatusAsync(songVersioned, song);
            }
            catch
            {
                songVersioned.IsOpeningSong = false;
                songVersioned.Status = "Error";
                throw;
            }
            
        }

        public async Task RevertSongAsync(SongVersioned songVersioned)
        {
            try
            {
                songVersioned.IsRevertingSong = true;
                Song song = _songsManager.FindSong(songVersioned.Title);
                await _songsManager.RevertSongAsync(song);
                await RefreshSongStatusAsync(songVersioned, song);
                songVersioned.IsRevertingSong = false;
            }
            catch
            {
                songVersioned.IsRevertingSong = false;
                songVersioned.Status = "Error";
                throw;
            }
            
        }

        public async Task UploadNewSongVersionAsync(SongVersioned songVersioned, string changeTitle, string changeDescription, bool compo, bool mix, bool mastering)
        {
            try
            {
                songVersioned.IsUploadingSong = true;
                Song song = _songsManager.FindSong(songVersioned.Title);
                await _songsManager.UploadNewSongVersionAsync(song, changeTitle, changeDescription, compo, mix, mastering);
                await RefreshSongVersionedAsync(songVersioned, song);
                songVersioned.IsUploadingSong = false;
            }
            catch
            {
                songVersioned.IsUploadingSong = false;
                songVersioned.Status = "Error";
                throw;
            }
        }

        public string ShareSong(SongVersioned songVersioned)
        {
            try
            {
                Song song = _songsManager.FindSong(songVersioned.Title);
                return _songsManager.ShareSong(song);
            }
            catch
            {
                songVersioned.Status = "Error";
                throw;
            }
                      
        }

        public async Task RefreshSongsVersionedAsync()
        {
           var refreshSongVersionedTasks = new List<Task>();
            foreach (var songVersioned in SongsVersioned)
            {
                Song song = _songsManager.FindSong(songVersioned.Title);
                Task refreshSongVersionedTask = RefreshSongVersionedAsync(songVersioned, song);
                refreshSongVersionedTasks.Add(refreshSongVersionedTask);
            }
            foreach(var task in refreshSongVersionedTasks)
            {
                await task;
            }
        }

        public void RenameSong(SongVersioned songVersioned, string title)
        {
            Song song = _songsManager.FindSong(songVersioned.Title);
            _songsManager.RenameSong(song,title);
            songVersioned.Title = title;
        }

        private void IntializeSongsVersioned()
        {
            if (_songsManager.SongList != null)
            {
                foreach (Song song in _songsManager.SongList)
                {
                    SongVersioned songVersioned = new SongVersioned(song.Title);
                    SongsVersioned.Add(songVersioned);
                }
            }
        }

        private async Task RefreshSongVersionedAsync(SongVersioned songVersioned, Song song)
        {
            try
            {
                songVersioned.IsRefreshingSong = true;
                await RefreshSongCurrentVersionAsync(songVersioned, song);
                await RefreshSongStatusAsync(songVersioned, song);
                await RefreshSongVersionsAsync(songVersioned, song);
                songVersioned.IsRefreshingSong = false;
            }
            catch
            {
                songVersioned.Status = "Error";
                songVersioned.IsRefreshingSong = false;
                throw;
            }
        }

        private async Task RefreshSongCurrentVersionAsync(SongVersioned songVersioned, Song song)
        {
            SongVersion songVersion = await _songsManager.CurrentVersionAsync(song);
            FillVersion(songVersioned.CurrentVersion, songVersion);
        }

        private async Task RefreshSongVersionsAsync(SongVersioned songVersioned, Song song)
        {
            songVersioned.Versions.Clear();
            List<Models.SongVersion> versionsModels = await _songsManager.VersionsAsync(song);
            FillVersions(songVersioned.Versions, versionsModels);

            songVersioned.UpcomingVersions.Clear();
            List<Models.SongVersion> upcomingVersionsModels = await _songsManager.UpcomingVersionsAsync(song);
            FillVersions(songVersioned.UpcomingVersions, upcomingVersionsModels);
        }

        private static void FillVersions(ObservableCollection<Version> UpcomingVersions, List<SongVersion> upcomingVersionsModels)
        {
            foreach (var versionModel in upcomingVersionsModels)
            {
                ViewModels.Version versionViewModels = new ViewModels.Version();
                FillVersion(versionViewModels, versionModel);
                UpcomingVersions.Insert(0, versionViewModels);
            }
        }

        private static void FillVersion(Version Version, SongVersion songVersion)
        {
            Version.Number = songVersion.Number;
            Version.Description = songVersion.Description;
            Version.Author = songVersion.Author;
            Version.Date = songVersion.Date.ToString("d", CultureInfo.GetCultureInfo("en-US"));
        }

        private async Task RefreshSongStatusAsync(SongVersioned songVersioned, Song song)
        {
            await _songsManager.RefreshSongStatusAsync(song);
            if (song.Status.state == SongStatus.State.locked)
            {
                songVersioned.Status = "Locked by " + song.Status.whoLocked;
            }
            else if (song.Status.state == SongStatus.State.updatesAvailable)
            {
                songVersioned.Status = "Updates Available";
            }
            else if (song.Status.state == SongStatus.State.upToDate)
            {
                songVersioned.Status = string.Empty;
                songVersioned.EnableStatus = false;
            }
        }

        public ObservableCollection<SongVersioned> SongsVersioned;
        private bool _isAddingSong;
        public bool IsAddingSong
        {
            get
            {
                return _isAddingSong;
            }
            set
            {
                SetProperty(ref _isAddingSong, value);
            }
        }

        private readonly IFileManager _fileManager;
        private readonly ISongsManager _songsManager;
    }
}