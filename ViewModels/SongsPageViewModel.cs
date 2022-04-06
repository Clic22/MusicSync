using App1.Models;
using App1.Models.Ports;
using System.Collections.ObjectModel;
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

        public async Task<string> updateAllSongsAsync()
        {
            string errorMessage = string.Empty;
            foreach (SongVersioned songVersioned in SongsVersioned)
            {
                errorMessage = await updateSongAsync(songVersioned);
                if (errorMessage != string.Empty)
                {
                    break;
                }
            }
            return errorMessage;
        }

        public SongVersioned addLocalSong(string songTitle, string songFile, string songLocalPath)
        {
            IsAddingSong = true;
            songLocalPath = FileManager.FormatPath(songLocalPath);
            SongsManager.addLocalSong(songTitle, songFile, songLocalPath);
            SongVersioned songVersioned = new SongVersioned(songTitle);
            SongsVersioned.Add(songVersioned);
            IsAddingSong = false;
            return songVersioned;
        }

        public async Task<string> addSharedSongAsync(string songTitle, string sharedLink, string songLocalPath)
        {
            IsAddingSong = true;
            songLocalPath = FileManager.FormatPath(songLocalPath);
            string errorMessage = await SongsManager.addSharedSongAsync(songTitle, sharedLink, songLocalPath);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                IsAddingSong = false;
                return errorMessage;
            }
            SongVersioned songVersioned = new SongVersioned(songTitle);
            SongsVersioned.Add(songVersioned);
            await updateSongAsync(songVersioned);
            IsAddingSong = false;
            return string.Empty;
        }

        public async Task deleteSongAsync(SongVersioned songVersioned)
        {
            Song song = SongsManager.findSong(songVersioned.Title);
            await SongsManager.deleteSongAsync(song);
            SongsVersioned.Remove(songVersioned);
        }

        public async Task<string> updateSongAsync(SongVersioned songVersioned)
        {
            songVersioned.IsUpdatingSong = true;
            Song song = SongsManager.findSong(songVersioned.Title);
            string errorMessage = await SongsManager.updateSongAsync(song);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                songVersioned.IsUpdatingSong = false;
                return errorMessage;
            }
            await refreshSongVersionedAsync(songVersioned, song);
            songVersioned.IsUpdatingSong = false;
            return errorMessage;
        }

        public async Task<bool> openSongAsync(SongVersioned songVersioned)
        {
            songVersioned.IsOpeningSong = true;
            Song song = SongsManager.findSong(songVersioned.Title);
            bool errorMessage = await SongsManager.openSongAsync(song);
            if (!errorMessage)
            {
                songVersioned.IsOpeningSong = false;
                return errorMessage;
            }
            songVersioned.IsOpeningSong = false;
            await refreshSongStatusAsync(songVersioned, song);
            return errorMessage;
        }

        public async Task<string> revertSongAsync(SongVersioned songVersioned)
        {
            songVersioned.IsRevertingSong = true;
            Song song = SongsManager.findSong(songVersioned.Title);
            string errorMessage = await SongsManager.revertSongAsync(song);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                songVersioned.IsRevertingSong = false;
                return errorMessage;
            }
            await refreshSongStatusAsync(songVersioned, song);
            songVersioned.IsRevertingSong = false;
            return errorMessage;
        }

        public async Task<string> uploadNewSongVersionAsync(SongVersioned songVersioned, string changeTitle, string changeDescription, bool compo, bool mix, bool mastering)
        {
            songVersioned.IsUploadingSong = true;
            Song song = SongsManager.findSong(songVersioned.Title);
            string errorMessage = await SongsManager.uploadNewSongVersionAsync(song, changeTitle, changeDescription, compo, mix, mastering);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                songVersioned.IsUploadingSong = false;
                return errorMessage;
            }
            await refreshSongVersionedAsync(songVersioned, song);
            songVersioned.IsUploadingSong = false;
            return errorMessage;
        }

        public async Task<string> shareSongAsync(SongVersioned songVersioned)
        {
            Song song = SongsManager.findSong(songVersioned.Title);
            return await SongsManager.shareSongAsync(song);
        }

        public async Task refreshSongsVersionedAsync()
        {
            foreach(var songVersioned in SongsVersioned)
            {
                Song song = SongsManager.findSong(songVersioned.Title);
                await refreshSongVersionedAsync(songVersioned, song);
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
            await refreshSongStatusAsync(songVersioned, song);
            await refreshSongCurrentVersionAsync(songVersioned, song);
            await refreshSongVersionsAsync(songVersioned, song);
        }

        private async Task refreshSongCurrentVersionAsync(SongVersioned songVersioned, Song song)
        {
            SongVersion songVersion = await SongsManager.currentVersionAsync(song);
            songVersioned.CurrentVersion.Number = songVersion.Number;
            songVersioned.CurrentVersion.Description = songVersion.Description;
            songVersioned.CurrentVersion.Author = songVersion.Author;
        }

        private async Task refreshSongVersionsAsync(SongVersioned songVersioned, Song song)
        {
            songVersioned.Versions.Clear();
            List<Models.SongVersion> versionsModels = await SongsManager.versionsAsync(song);
            foreach (var versionModel in versionsModels)
            {
                ViewModels.Version versionViewModels = new ViewModels.Version();
                versionViewModels.Number = versionModel.Number;
                versionViewModels.Description = versionModel.Description;
                versionViewModels.Author = versionModel.Author;
                songVersioned.Versions.Insert(0, versionViewModels);
            }
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