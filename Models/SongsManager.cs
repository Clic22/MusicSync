using App1.Models.Ports;
using System.Diagnostics;
using System.Threading.Tasks;

namespace App1.Models
{
    public class SongsManager : ISongsManager
    {
        public SongsManager(IVersionTool NewVersionTool, ISaver NewSaver)
        {
            VersionTool = NewVersionTool;
            Saver = NewSaver;
            SongList = new SongsStorage(Saver);
            Locker = new Locker(VersionTool);
        }

        public async Task<string> updateAllSongsAsync()
        {
            foreach (Song song in SongList)
            {
                string errorMessage = await updateSongAsync(song);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    return errorMessage;
                }
            }
            return string.Empty;
        }

        public async Task<string> updateSongAsync(Song song)
        {
            string errorMessage = await VersionTool.updateSongAsync(song);
            Locker.updateSongStatus(song);
            return errorMessage;
        }

        public async Task<string> uploadNewSongVersion(Song song, string changeTitle, string changeDescription)
        {
            string errorMessage = string.Empty;
            if (await Locker.unlockSongAsync(song, Saver.savedUser()))
            {
                errorMessage = await VersionTool.uploadSongAsync(song, changeTitle, changeDescription);
            }
            return errorMessage;
        }

        public void addSong(string songTitle, string songFile, string songLocalPath)
        {
            Song song = new Song(songTitle, songFile, songLocalPath);
            SongList.addNewSong(song);
            Locker.updateSongStatus(song);
        }

        public async Task deleteSong(Song song)
        {
            await Locker.unlockSongAsync(song, Saver.savedUser());
            SongList.deleteSong(song);
        }

        public async Task<(bool,string)> openSongAsync(Song song)
        {
            string errorMessage = await updateSongAsync(song);
            if (string.IsNullOrEmpty(errorMessage))
            {
                (bool, string)  locked = await Locker.lockSongAsync(song, Saver.savedUser());
                if (Locker.isLockedByUser(song, Saver.savedUser()))
                {
                    openSongWithDAW(song);
                    return (true, string.Empty);
                }
                return locked;
            }
            else
            {
                return (false, errorMessage);
            }
            
        }

        public async Task<string> revertSongAsync(Song song)
        {
            string errorMessage = await updateSongAsync(song);
            if (string.IsNullOrEmpty(errorMessage) && await Locker.unlockSongAsync(song, Saver.savedUser()))
            {
                errorMessage = await VersionTool.revertSongAsync(song);
            }
            return errorMessage;
        }

        private static void openSongWithDAW(Song song)
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(song.LocalPath + @"\" + song.File)
            {
                UseShellExecute = true
            };
            p.Start();
        }

        public Song? findSong(string songTitle)
        {
            return SongList.Find(song => song.Title == songTitle);
        }

        public SongsStorage SongList { get; private set; }
        private readonly IVersionTool VersionTool;
        private readonly Locker Locker;
        private readonly ISaver Saver;
    }
}
