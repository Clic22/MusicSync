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
            string songGuid = Guid.NewGuid().ToString();
            Song song = addSong(songTitle, songFile, songLocalPath, songGuid);
            await VersionTool.uploadSongAsync(song, "First Upload", String.Empty, "1.0.0");
        }

        public async Task addSharedSongAsync(string songTitle, string sharedLink, string downloadLocalPath)
        {
            string songPath = FileManager.FormatPath(downloadLocalPath + songTitle);
            await VersionTool.downloadSharedSongAsync(sharedLink, songPath);
            string songGuid = VersionTool.guidFromSharedLink(sharedLink);
            string? songFile = await FileManager.findFileNameBasedOnExtensionAsync(songPath, ".song");
            if (songFile != null)
            {
                addSong(songTitle, songFile, songPath, songGuid);
            }
            await refreshSongStatusAsync(findSong(songTitle));
        }

        public void renameSong(Song song, string newSongTitle)
        {
            string formerLocalPath = song.LocalPath;
            string newLocalPath = FileManager.FormatPath(formerLocalPath.Replace(song.Title + '\\', "") + newSongTitle);
            FileManager.RenameFolder(formerLocalPath, newLocalPath); 
            song.LocalPath = newLocalPath;
            song.Title = newSongTitle;
            string formerFile = song.File;
            string newFile = newSongTitle + ".song";
            FileManager.RenameFile(formerFile, newFile, song.LocalPath);
            song.File = newSongTitle + ".song";
            Saver.saveSong(song);
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

        public async Task<List<SongVersion>> upcomingVersionsAsync(Song song)
        {
            return await VersionTool.upcomingVersionsAsync(song);
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

        private Song addSong(string songTitle, string songFile, string songLocalPath, string songGuid)
        {
            Song song = new Song(songTitle, songFile, songLocalPath, songGuid);
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
