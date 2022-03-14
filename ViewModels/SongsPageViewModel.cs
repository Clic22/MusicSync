using App1.Adapters;
using App1.Models;
using App1.Models.Ports;
using System.Collections.ObjectModel;

namespace App1.ViewModels
{
    public class SongsPageViewModel
    {
        public SongsPageViewModel() { 
            SongsVersioned = new ObservableCollection<SongVersioned>();
            Saver = new LocalSettingsSaver();
            VersionTool = new GitSongVersioning();
            SongsManager = new SongsManager(VersionTool, Saver);
            intializeSongsVersioned();
        }

        public async Task<string> updateAllSongsAsync()
        {
            string errorMessage = string.Empty;
            foreach (SongVersioned songVersioned in SongsVersioned)
            {
                Song? song = SongsManager.SongList.Find(song => song.Title == songVersioned.Title);
                if (song != null)
                {
                    errorMessage = await SongsManager.updateSongAsync(song);
                    refreshSongVersioned(song);
                    if (errorMessage != string.Empty)
                    {
                        return errorMessage;
                    }
                }
                else
                {
                     return errorMessage = "Error: Song not found";
                }  
            }
            return errorMessage;
        }

        public void addSong(string songTitle, string songFile, string songLocalPath)
        {
            SongsManager.addSong(songTitle, songFile, songLocalPath);
            SongsVersioned.Add(new SongVersioned(songTitle));
            Song? song = SongsManager.SongList.Find(song => song.Title == songTitle);
            refreshSongVersioned(song);
        }

        public async Task deleteSong(SongVersioned songVersioned)
        {
            Song? song = SongsManager.SongList.Find(song => song.Title == songVersioned.Title);
            if (song != null)
                await SongsManager.deleteSong(song);
            SongsVersioned.Remove(songVersioned);
        }

        public async Task<string> updateSongAsync(SongVersioned songVersioned)
        {
            Song? song = SongsManager.SongList.Find(song => song.Title == songVersioned.Title);
            string errorMessage = string.Empty;
            if (song != null)
            {
                errorMessage = await SongsManager.updateSongAsync(song);
                refreshSongVersioned(song);
            }  
            else
            {
                errorMessage =  "Error: Song not found";
            }
            return errorMessage;
        }

        public async Task<(bool, string)> openSongAsync(SongVersioned songVersioned)
        {
            Song? song = SongsManager.SongList.Find(song => song.Title == songVersioned.Title);
            (bool, string) errorMessage = await SongsManager.openSongAsync(song);
            refreshSongVersioned(song);
            return errorMessage;
        }

        public async Task<string> revertSongAsync(SongVersioned songVersioned)
        {
            Song? song = SongsManager.SongList.Find(song => song.Title == songVersioned.Title);
            string errorMessage = await SongsManager.revertSongAsync(song);
            refreshSongVersioned(song);
            return errorMessage;
        }

        public async Task<string> uploadNewSongVersion(SongVersioned songVersioned, string changeTitle, string changeDescription)
        {
            Song? song = SongsManager.SongList.Find(song => song.Title == songVersioned.Title);
            string errorMessage = await SongsManager.uploadNewSongVersion(song, changeTitle, changeDescription);
            refreshSongVersioned(song);
            return errorMessage;
        }

        private Task intializeSongsVersioned()
        {
            return Task.Run(() =>
            {
                foreach (Song song in SongsManager.SongList)
                {
                    SongVersioned songVersioned = new SongVersioned(song.Title);
                    SongsVersioned.Add(songVersioned);
                }
            });
        }

        private void refreshSongVersioned(Song song)
        {
            SongVersioned songVersioned = SongsVersioned.First(songVersioned => songVersioned.Title == song.Title);
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
        private ISaver Saver;
        private IVersionTool VersionTool;
        private SongsManager SongsManager;
    }
}