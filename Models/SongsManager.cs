using App1.Adapters;
using System.Diagnostics;
using System.Threading.Tasks;

namespace App1
{
    public class SongsManager
    {
        public SongsManager()
        {
            VersionTool = new GitSongVersioning();
            SongList = new SongsStorage();
            Locker = new Locker();
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

        public async void uploadNewSongVersion(Song song, string changeTitle, string changeDescription)
        {
            if (Locker.isLockedByUser(song))
            {
                Locker.unlockSong(song);
                await VersionTool.uploadSongAsync(song, changeTitle, changeDescription);
            }
        }

        public void addSong(string songTitle, string songFile, string songLocalPath)
        {
            Song song = new Song(songTitle, songFile, songLocalPath);
            SongList.addNewSong(song);
        }

        public void deleteSong(Song song)
        {
            if (Locker.isLockedByUser(song))
            {
                Locker.unlockSong(song);
            }
            SongList.deleteSong(song);
        }

        public async Task<bool> openSong(Song song)
        {
            await updateSongAsync(song);
            if (song.status == Song.SongStatus.upToDate)
            {
                Locker.lockSong(song);
            }
            if (Locker.isLockedByUser(song))
            {
                openSongWithDAW(song);
                return true;
            }
            return false;
        }

        public async void revertSong(Song song)
        {
            await updateSongAsync(song);
            if (Locker.isLockedByUser(song))
            {
                await VersionTool.revertSongAsync(song);
                Locker.unlockSong(song);
            }
        }

        private static void openSongWithDAW(Song song)
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(song.localPath + @"\" + song.file)
            {
                UseShellExecute = true
            };
            p.Start();
        }

        private IVersionTool VersionTool;
        public SongsStorage SongList { get; private set; }
        private Locker Locker;
    }
}