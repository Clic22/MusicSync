using App1.Models.Ports;
using System.Diagnostics;

namespace App1.Models
{
    public class SongsManager : ISongsManager
    {
        public SongsManager(IVersionTool NewVersionTool, ISaver NewSaver, IFileManager NewFileManager)
        {
            VersionTool = NewVersionTool;
            Saver = NewSaver;
            FileManager = NewFileManager;
            SongList = new SongsStorage(Saver);
            Locker = new Locker(VersionTool);
        }

        public async Task<string> updateSongAsync(Song song)
        {
            string errorMessage = await VersionTool.updateSongAsync(song);
            Locker.updateSongStatus(song);
            return errorMessage;
        }

        public async Task<string> uploadNewSongVersionAsync(Song song, string changeTitle, string changeDescription, bool compo, bool mix, bool mastering)
        {
            string errorMessage = string.Empty;
            if (await Locker.unlockSongAsync(song, Saver.savedUser()))
            {
                string versionNumber = await VersionTool.newVersionNumberAsync(song, compo, mix, mastering);
                errorMessage = await VersionTool.uploadSongAsync(song, changeTitle, changeDescription, versionNumber);
            }
            return errorMessage;
        }

        public void addLocalSong(string songTitle, string songFile, string songLocalPath)
        {
            Song song = new Song(songTitle, songFile, songLocalPath);
            SongList.addNewSong(song);
            Locker.updateSongStatus(song);
        }

        public async Task<string> addSharedSongAsync(string songTitle, string sharedLink, string downloadLocalPath)
        {
            string errorMessage = await VersionTool.downloadSharedSongAsync(songTitle, sharedLink, downloadLocalPath);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return errorMessage;
            }
            string localPath = downloadLocalPath + @"\" + songTitle;
            string songFile = await FileManager.findFileNameBasedOnExtensionAsync(localPath,".song");
            if (string.IsNullOrEmpty(songFile))
            {
                return "Song File not Found in " + localPath;
            }
            addLocalSong(songTitle, songFile, localPath);
            return string.Empty;
        }

        public async Task deleteSongAsync(Song song)
        {
            await Locker.unlockSongAsync(song, Saver.savedUser());
            SongList.deleteSong(song);
        }

        public async Task<(bool, string)> openSongAsync(Song song)
        {
            string errorMessage = await updateSongAsync(song);
            if (string.IsNullOrEmpty(errorMessage))
            {
                (bool, string) locked = await Locker.lockSongAsync(song, Saver.savedUser());
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

        public Song findSong(string songTitle)
        {
            Song? song = SongList.Find(song => song.Title == songTitle);
            if (song != null)
            {
                return song;
            }
            else
            {
                throw new InvalidOperationException("Song not Found in SongList");
            }
        }

        public async Task<SongVersion> currentVersionAsync(Song song)
        {
            return await VersionTool.currentVersionAsync(song);
        }

        public async Task<List<SongVersion>> versionsAsync(Song song)
        {
            return await VersionTool.versionsAsync(song);
        }

        public async Task<string> shareSongAsync(Song song)
        {
            return await VersionTool.shareSongAsync(song);
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
        private readonly IVersionTool VersionTool;
        private readonly Locker Locker;
        private readonly ISaver Saver;
        private readonly IFileManager FileManager;
    }
}
