using App1.Adapters;
using App1.Models;
using App1.Models.Ports;
using System.Collections.ObjectModel;

namespace App1.ViewModels
{
    public class SongsPageViewModel
    {
        public SongsPageViewModel() { 
            SongsVersioned = new ObservableCollection<SongVersioned> { new SongVersioned() };
            Saver = new LocalSettingsSaver();
            VersionTool = new GitSongVersioning();
            SongsManager = new SongsManager(VersionTool, Saver);
        }

        public Task<string> updateAllSongsAsync()
        {
            throw new NotImplementedException();
        }

        public void addSong(string text1, string text2, string text3)
        {
            throw new NotImplementedException();
        }

        public Task deleteSong(SongVersioned song)
        {
            throw new NotImplementedException();
        }

        public Task<string> updateSongAsync(SongVersioned song)
        {
            throw new NotImplementedException();
        }

        public Task<(bool, string)> openSongAsync(SongVersioned song)
        {
            throw new NotImplementedException();
        }

        public Task<string> revertSongAsync(SongVersioned song)
        {
            throw new NotImplementedException();
        }

        public Task<string> uploadNewSongVersion(SongVersioned song, string text1, string text2)
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<SongVersioned> SongsVersioned;
        private ISaver Saver;
        private IVersionTool VersionTool;
        private SongsManager SongsManager;
    }
}