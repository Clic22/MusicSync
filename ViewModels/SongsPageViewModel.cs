using App1.Models;
using App1.Models.Ports;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace App1.ViewModels
{
    public class SongsPageViewModel : Bindable
    {
        public SongsPageViewModel(ISongsManager songsManager) { 
            SongsVersioned = new ObservableCollection<SongVersioned>();
            SongsManager = songsManager;
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
            SongsManager.addLocalSong(songTitle, songFile, songLocalPath);
            SongVersioned songVersioned = new SongVersioned(songTitle);
            SongsVersioned.Add(songVersioned);
            IsAddingSong = false;
            return songVersioned;
        }

        public async Task<string> addSharedSongAsync(string songTitle, string sharedLink, string songLocalPath)
        {
            IsAddingSong = true;
            string errorMessage = await SongsManager.addSharedSongAsync(songTitle, sharedLink, songLocalPath);
            if (errorMessage != string.Empty)
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
            await refreshSongVersionedAsync(songVersioned,song);
            songVersioned.IsUpdatingSong = false;
            return errorMessage;
        }

        public async Task<(bool, string)> openSongAsync(SongVersioned songVersioned)
        {
            songVersioned.IsOpeningSong = true;
            Song song = SongsManager.findSong(songVersioned.Title);
            (bool, string) errorMessage = await SongsManager.openSongAsync(song);
            songVersioned.IsOpeningSong = false;
            refreshSongStatus(songVersioned, song);
            return errorMessage;
        }

        public async Task<string> revertSongAsync(SongVersioned songVersioned)
        {
            songVersioned.IsRevertingSong = true;
            Song song = SongsManager.findSong(songVersioned.Title);
            string errorMessage = await SongsManager.revertSongAsync(song);
            refreshSongStatus(songVersioned, song);
            songVersioned.IsRevertingSong = false;
            return errorMessage;
        }

        public async Task<string> uploadNewSongVersionAsync(SongVersioned songVersioned, string changeTitle, string changeDescription, bool compo, bool mix, bool mastering)
        {
            songVersioned.IsUploadingSong = true;
            Song song = SongsManager.findSong(songVersioned.Title);
            string errorMessage = await SongsManager.uploadNewSongVersionAsync(song, changeTitle, changeDescription, compo, mix, mastering);
            await refreshSongVersionedAsync(songVersioned,song);
            songVersioned.IsUploadingSong = false;
            return errorMessage;
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
            refreshSongStatus(songVersioned, song);
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
            List<Models.SongVersion> versionsModels= await SongsManager.versionsAsync(song);
            foreach (var versionModel in versionsModels)
            {
                ViewModels.Version versionViewModels = new ViewModels.Version();
                versionViewModels.Number = versionModel.Number;
                versionViewModels.Description = versionModel.Description;
                versionViewModels.Author = versionModel.Author;
                songVersioned.Versions.Insert(0, versionViewModels);
            }
        }

        private void refreshSongStatus(SongVersioned songVersioned, Song song)
        {
            if (song.Status.state == SongStatus.State.locked)
            {
                songVersioned.Status = "Locked by " + song.Status.whoLocked;
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

        private readonly ISongsManager SongsManager;
    }
}