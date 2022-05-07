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
            SongsManager = songsManager;
            FileManager = new FileManager();
            intializeSongsVersioned();
        }

        public async Task<SongVersioned> addLocalSongAsync(string songTitle, string songFile, string songLocalPath)
        {
            try
            {
                IsAddingSong = true;
                songLocalPath = FileManager.FormatPath(songLocalPath);
                await SongsManager.addLocalSongAsync(songTitle, songFile, songLocalPath);
                SongVersioned songVersioned = new SongVersioned(songTitle);
                SongsVersioned.Add(songVersioned);
                Song song = SongsManager.findSong(songVersioned.Title);
                await refreshSongVersionedAsync(songVersioned, song);
                IsAddingSong = false;
                return songVersioned;
            }
            catch
            {
                IsAddingSong = false;
                throw;
            }
            
        }

        public async Task addSharedSongAsync(string songTitle, string sharedLink, string songLocalPath)
        {
            try
            {
                IsAddingSong = true;
                songLocalPath = FileManager.FormatPath(songLocalPath);
                await SongsManager.addSharedSongAsync(songTitle, sharedLink, songLocalPath);
                SongVersioned songVersioned = new SongVersioned(songTitle);
                SongsVersioned.Add(songVersioned);
                await updateSongAsync(songVersioned);
                IsAddingSong = false;
            }
            catch
            {
                IsAddingSong = false;
                throw;
            }
        }

        public async Task deleteSongAsync(SongVersioned songVersioned)
        {
            try
            {
                Song song = SongsManager.findSong(songVersioned.Title);
                await SongsManager.deleteSongAsync(song);
                SongsVersioned.Remove(songVersioned);
            }
            catch
            {
                songVersioned.Status = "Error";
                throw;
            }
              
        }

        public async Task updateSongAsync(SongVersioned songVersioned)
        {
            try
            {
                songVersioned.IsUpdatingSong = true;
                Song song = SongsManager.findSong(songVersioned.Title);
                await SongsManager.updateSongAsync(song);
                await refreshSongVersionedAsync(songVersioned, song);
                songVersioned.IsUpdatingSong = false;
            }
            catch
            {
                songVersioned.IsUpdatingSong = false;
                songVersioned.Status = "Error";
                throw;
            }
        }

        public async Task openSongAsync(SongVersioned songVersioned)
        {
            try
            {
                songVersioned.IsOpeningSong = true;
                Song song = SongsManager.findSong(songVersioned.Title);
                await SongsManager.openSongAsync(song);
                songVersioned.IsOpeningSong = false;
                await refreshSongStatusAsync(songVersioned, song);
            }
            catch
            {
                songVersioned.IsOpeningSong = false;
                songVersioned.Status = "Error";
                throw;
            }
            
        }

        public async Task revertSongAsync(SongVersioned songVersioned)
        {
            try
            {
                songVersioned.IsRevertingSong = true;
                Song song = SongsManager.findSong(songVersioned.Title);
                await SongsManager.revertSongAsync(song);
                await refreshSongStatusAsync(songVersioned, song);
                songVersioned.IsRevertingSong = false;
            }
            catch
            {
                songVersioned.IsRevertingSong = false;
                songVersioned.Status = "Error";
                throw;
            }
            
        }

        public async Task uploadNewSongVersionAsync(SongVersioned songVersioned, string changeTitle, string changeDescription, bool compo, bool mix, bool mastering)
        {
            try
            {
                songVersioned.IsUploadingSong = true;
                Song song = SongsManager.findSong(songVersioned.Title);
                await SongsManager.uploadNewSongVersionAsync(song, changeTitle, changeDescription, compo, mix, mastering);
                await refreshSongVersionedAsync(songVersioned, song);
                songVersioned.IsUploadingSong = false;
            }
            catch
            {
                songVersioned.IsUploadingSong = false;
                songVersioned.Status = "Error";
                throw;
            }
        }

        public async Task<string> shareSongAsync(SongVersioned songVersioned)
        {
            try
            {
                Song song = SongsManager.findSong(songVersioned.Title);
                return await SongsManager.shareSongAsync(song);
            }
            catch
            {
                songVersioned.Status = "Error";
                throw;
            }
                      
        }

        public async Task refreshSongsVersionedAsync()
        {
            List<Task> refreshSongVersionedTasks = new List<Task>();
            foreach (var songVersioned in SongsVersioned)
            {
                Song song = SongsManager.findSong(songVersioned.Title);
                Task refreshSongVersionedTask = refreshSongVersionedAsync(songVersioned, song);
                refreshSongVersionedTasks.Add(refreshSongVersionedTask);
            }
            foreach(var task in refreshSongVersionedTasks)
            {
                await task;
            }
        }

        private void intializeSongsVersioned()
        {
            if (SongsManager.SongList != null)
            {
                foreach (Song song in SongsManager.SongList)
                {
                    SongVersioned songVersioned = new SongVersioned(song.Title);
                    SongsVersioned.Add(songVersioned);
                }
            }
        }

        private async Task refreshSongVersionedAsync(SongVersioned songVersioned, Song song)
        {
            try
            {
                songVersioned.IsRefreshingSong = true;
                await refreshSongCurrentVersionAsync(songVersioned, song);
                await refreshSongStatusAsync(songVersioned, song);
                await refreshSongVersionsAsync(songVersioned, song);
                songVersioned.IsRefreshingSong = false;
            }
            catch
            {
                songVersioned.Status = "Error";
                songVersioned.IsRefreshingSong = false;
                throw;
            }
        }

        private async Task refreshSongCurrentVersionAsync(SongVersioned songVersioned, Song song)
        {
            SongVersion songVersion = await SongsManager.currentVersionAsync(song);
            fillVersion(songVersioned.CurrentVersion, songVersion);
        }

        private async Task refreshSongVersionsAsync(SongVersioned songVersioned, Song song)
        {
            songVersioned.Versions.Clear();
            List<Models.SongVersion> versionsModels = await SongsManager.versionsAsync(song);
            fillVersions(songVersioned.Versions, versionsModels);

            songVersioned.UpcomingVersions.Clear();
            List<Models.SongVersion> upcomingVersionsModels = await SongsManager.upcomingVersionsAsync(song);
            fillVersions(songVersioned.UpcomingVersions, upcomingVersionsModels);
        }

        private static void fillVersions(ObservableCollection<Version> UpcomingVersions, List<SongVersion> upcomingVersionsModels)
        {
            foreach (var versionModel in upcomingVersionsModels)
            {
                ViewModels.Version versionViewModels = new ViewModels.Version();
                fillVersion(versionViewModels, versionModel);
                UpcomingVersions.Insert(0, versionViewModels);
            }
        }

        private static void fillVersion(Version Version, SongVersion songVersion)
        {
            Version.Number = songVersion.Number;
            Version.Description = songVersion.Description;
            Version.Author = songVersion.Author;
            Version.Date = songVersion.Date.ToString("d", CultureInfo.GetCultureInfo("en-US"));
        }

        private async Task refreshSongStatusAsync(SongVersioned songVersioned, Song song)
        {
            await SongsManager.refreshSongStatusAsync(song);
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
        private bool isAddingSong_;
        public bool IsAddingSong
        {
            get
            {
                return isAddingSong_;
            }
            set
            {
                SetProperty(ref isAddingSong_, value);
            }
        }

        private readonly IFileManager FileManager;
        private readonly ISongsManager SongsManager;
    }
}