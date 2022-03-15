using App1.Models;
using App1.Models.Ports;
using System.Collections.ObjectModel;

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
                Song song = SongsManager.findSong(songVersioned.Title);
                errorMessage = await SongsManager.updateSongAsync(song);
                if (errorMessage != string.Empty)
                {
                    return errorMessage;
                }
                refreshSongVersioned(songVersioned, song);
            }
            return errorMessage;
        }

        public void addSong(string songTitle, string songFile, string songLocalPath)
        {
            SongsManager.addSong(songTitle, songFile, songLocalPath);
            SongsVersioned.Add(new SongVersioned(songTitle));
            SongVersioned songVersioned = SongsVersioned.First(songVersioned => songVersioned.Title == songTitle);
            Song song = SongsManager.findSong(songTitle);
            refreshSongVersioned(songVersioned,song);
        }

        public async Task deleteSongAsync(SongVersioned songVersioned)
        {
            Song song = SongsManager.findSong(songVersioned.Title);
            await SongsManager.deleteSongAsync(song);
            SongsVersioned.Remove(songVersioned);
        }

        public async Task<string> updateSongAsync(SongVersioned songVersioned)
        {
            Song song = SongsManager.findSong(songVersioned.Title);
            string errorMessage = await SongsManager.updateSongAsync(song);
            refreshSongVersioned(songVersioned,song);
            return errorMessage;
        }

        public async Task<(bool, string)> openSongAsync(SongVersioned songVersioned)
        {
            Song song = SongsManager.findSong(songVersioned.Title);
            (bool, string) errorMessage = await SongsManager.openSongAsync(song);
            refreshSongVersioned(songVersioned,song);
            return errorMessage;
        }

        public async Task<string> revertSongAsync(SongVersioned songVersioned)
        {
            Song song = SongsManager.findSong(songVersioned.Title);
            string errorMessage = await SongsManager.revertSongAsync(song);
            refreshSongVersioned(songVersioned,song);
            return errorMessage;
        }

        public async Task<string> uploadNewSongVersion(SongVersioned songVersioned, string changeTitle, string changeDescription)
        {
            Song song = SongsManager.findSong(songVersioned.Title);
            string errorMessage = await SongsManager.uploadNewSongVersion(song, changeTitle, changeDescription);
            refreshSongVersioned(songVersioned,song);
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

        private void refreshSongVersioned(SongVersioned songVersioned, Song song)
        {
            refreshSongStatus(songVersioned, song);
        }

        private void refreshSongStatus(SongVersioned songVersioned, Song song)
        {
            if (song.Status == Song.SongStatus.locked)
                songVersioned.Status = "Locked";
            else if (song.Status == Song.SongStatus.upToDate)
                songVersioned.Status = "Up to Date";
        }

        public ObservableCollection<SongVersioned> SongsVersioned;
        private readonly ISongsManager SongsManager;
    }
}