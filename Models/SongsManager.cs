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
            Locker = new Locker(VersionTool,FileManager);
        }

        public async Task updateSongAsync(Song song)
        {
            if (await VersionTool.updatesAvailableForSongAsync(song))
            {
                await VersionTool.updateSongAsync(song);
                await refreshSongStatusAsync(song);
            }
        }

        public async Task uploadNewSongVersionAsync(Song song, string changeTitle, string changeDescription, bool compo, bool mix, bool mastering)
        {
            if (await Locker.unlockSongAsync(song, Saver.savedUser()))
            {
                string versionNumber = await VersionTool.newVersionNumberAsync(song, compo, mix, mastering);
                await VersionTool.uploadSongAsync(song, changeTitle, changeDescription, versionNumber);
                await refreshSongStatusAsync(song);
            }
        }

        public async Task addLocalSongAsync(string songTitle, string songFile, string songLocalPath)
        {
            Song song = addSong(songTitle, songFile, songLocalPath);
            await VersionTool.uploadSongAsync(song, "First Upload", String.Empty, "1.0.0");
        }

        public async Task addSharedSongAsync(string songTitle, string sharedLink, string downloadLocalPath)
        {
            string songFolder = FileManager.FormatPath(songTitle);
            await VersionTool.downloadSharedSongAsync(songFolder, sharedLink, downloadLocalPath);
            string localPath = downloadLocalPath + songFolder;
            string songFile = await FileManager.findFileNameBasedOnExtensionAsync(localPath,".song");
            addSong(songTitle, songFile, localPath);
            await refreshSongStatusAsync(findSong(songTitle));
        }

        public async Task deleteSongAsync(Song song)
        {
            await Locker.unlockSongAsync(song, Saver.savedUser());
            SongList.deleteSong(song);
        }

        public async Task openSongAsync(Song song)
        {
            await updateSongAsync(song);

            bool lockedByUser = await Locker.lockSongAsync(song, Saver.savedUser());
            if (lockedByUser)
            {
                openSongWithDAW(song);
                await refreshSongStatusAsync(song);
            }
            else
            {
                throw new SongsManagerException("Song locked by " + song.Status.whoLocked);
            }
        }

        public async Task revertSongAsync(Song song)
        {
            if (await Locker.unlockSongAsync(song, Saver.savedUser()))
            {
                await VersionTool.revertSongAsync(song);
            }
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
                throw new SongsManagerException("Song not Found with title : " + songTitle);
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

        public async Task refreshSongStatusAsync(Song song)
        {
            if (await VersionTool.updatesAvailableForSongAsync(song))
            {
                song.Status.state = SongStatus.State.updatesAvailable;
            }
            else if (Locker.isLocked(song))
            {
                song.Status.state = SongStatus.State.locked;
                song.Status.whoLocked = Locker.whoLocked(song);
            }
            else
            {
                song.Status.state = SongStatus.State.upToDate;
            }
        }

        private Song addSong(string songTitle, string songFile, string songLocalPath)
        {
            Song song = new Song(songTitle, songFile, songLocalPath);
            SongList.addNewSong(song);
            return song;
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
