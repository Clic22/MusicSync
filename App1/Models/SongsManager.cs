using App1.Models.Ports;
using System.Diagnostics;
using System.Threading.Tasks;

namespace App1.Models
{
    public class SongsManager
    {
        public SongsManager(IVersionTool NewVersionTool, ISaver NewSaver)
        {
            VersionTool = NewVersionTool;
            Saver = NewSaver;
            SongList = new SongsStorage(Saver);
            Locker = new Locker(VersionTool);
        }

        public async Task updateAllSongsAsync()
        {
            foreach (Song song in SongList)
            {
                await updateSongAsync(song);
            }
        }

        public async Task updateSongAsync(Song song)
        {
            await VersionTool.updateSongAsync(song);
            Locker.updateSongStatus(song);
        }

        public async Task uploadNewSongVersion(Song song, string changeTitle, string changeDescription)
        {
            if (await Locker.unlockSongAsync(song, Saver.savedUser()))
            {
                await VersionTool.uploadSongAsync(song, changeTitle, changeDescription);
            }
        }

        public void addSong(string songTitle, string songFile, string songLocalPath)
        {
            Song song = new Song(songTitle, songFile, songLocalPath);
            SongList.addNewSong(song);
        }

        public async void deleteSong(Song song)
        {
            if (await Locker.unlockSongAsync(song, Saver.savedUser()))
            {
                SongList.deleteSong(song);
            }
        }

        public async Task<bool> openSong(Song song)
        {
            await updateSongAsync(song);
            if (song.Status == Song.SongStatus.upToDate)
            {
                await Locker.lockSongAsync(song, Saver.savedUser());
            }
            if (Locker.isLockedByUser(song, Saver.savedUser()))
            {
                openSongWithDAW(song);
                return true;
            }
            return false;
        }

        public async void revertSong(Song song)
        {
            await updateSongAsync(song);
            if (await Locker.unlockSongAsync(song, Saver.savedUser()))
            {
                await VersionTool.revertSongAsync(song);
            }
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

        public SongsStorage SongList { get; private set; }
        private IVersionTool VersionTool;
        private Locker Locker;
        private ISaver Saver;
    }
}