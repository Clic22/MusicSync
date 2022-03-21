using App1.Models;
using App1.Models.Ports;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace App1.ViewModels
{
    public class SongsPageViewModel
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

        public async Task addSongAsync
            (string songTitle, string songFile, string songLocalPath)
        {
            SongsManager.addSong(songTitle, songFile, songLocalPath);
            SongsVersioned.Add(new SongVersioned(songTitle));
            SongVersioned songVersioned = SongsVersioned.First(songVersioned => songVersioned.Title == songTitle);
            Song song = SongsManager.findSong(songTitle);
            await refreshSongVersionedAsync(songVersioned,song);
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
            Song song = SongsManager.findSong(songVersioned.Title);
            (bool, string) errorMessage = await SongsManager.openSongAsync(song);
            await refreshSongVersionedAsync(songVersioned,song);
            return errorMessage;
        }

        public async Task<string> revertSongAsync(SongVersioned songVersioned)
        {
            Song song = SongsManager.findSong(songVersioned.Title);
            string errorMessage = await SongsManager.revertSongAsync(song);
            await refreshSongVersionedAsync(songVersioned,song);
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
            await refreshSongVersionDescriptionAsync(songVersioned, song);
            await refreshSongVersionNumberAsync(songVersioned, song);
        }

        private async Task refreshSongVersionDescriptionAsync(SongVersioned songVersioned, Song song)
        {
            songVersioned.VersionDescription = await SongsManager.versionDescriptionAsync(song);
        }

        private async Task refreshSongVersionNumberAsync(SongVersioned songVersioned, Song song)
        {
            songVersioned.VersionNumber = await SongsManager.versionNumberAsync(song);
        }

        private void refreshSongStatus(SongVersioned songVersioned, Song song)
        {
            if (song.Status == Song.SongStatus.locked)
                songVersioned.Status = "Locked";
            else if (song.Status == Song.SongStatus.upToDate)
                songVersioned.Status = string.Empty;
        }

        public ObservableCollection<SongVersioned> SongsVersioned;
        private readonly ISongsManager SongsManager;
    }
}