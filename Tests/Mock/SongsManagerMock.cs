using App1.Models;
using App1.Models.Ports;

namespace App1Tests.Mock
{
    public class SongsManagerMock : ISongsManager
    {
        public SongsManagerMock(IVersionTool verionTool, ISaver saver)
        {
            this.saver = saver;
            this.versionTool = verionTool;
        }

        public SongsStorage SongList => throw new NotImplementedException();

        public void addSong(string songTitle, string songFile, string songLocalPath)
        {
            throw new Exception("addSong Called");
        }

        public Task deleteSong(Song song)
        {
            throw new Exception("deleteSong Called");
        }

        public Song? findSong(string songTitle)
        {
            throw new Exception("findSong Called");
        }

        public Task<(bool, string)> openSongAsync(Song song)
        {
            throw new Exception("openSongAsync Called");
        }

        public Task<string> revertSongAsync(Song song)
        {
            throw new Exception("revertSongAsync Called");
        }

        public Task<string> updateAllSongsAsync()
        {
            throw new Exception("updateAllSongsAsync Called");
        }

        public Task<string> updateSongAsync(Song song)
        {
            throw new Exception("updateSongAsync Called");
        }

        public Task<string> uploadNewSongVersion(Song song, string changeTitle, string changeDescription)
        {
            throw new Exception("uploadNewSongVersion Called");
        }

        private ISaver saver;
        private IVersionTool versionTool;
    }
}
