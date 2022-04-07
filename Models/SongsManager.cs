﻿using App1.Models.Ports;
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
            if (await VersionTool.updatesAvailableForSongAsync(song))
            {
                string errorMessage = await VersionTool.updateSongAsync(song);
                await refreshSongStatusAsync(song);
                return errorMessage;
            }
            return string.Empty;
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

        public async Task addLocalSongAsync(string songTitle, string songFile, string songLocalPath)
        {
            Song song = new Song(songTitle, songFile, songLocalPath);
            SongList.addNewSong(song);
            await refreshSongStatusAsync(song);
        }

        public async Task<string> addSharedSongAsync(string songTitle, string sharedLink, string downloadLocalPath)
        {
            string errorMessage = await VersionTool.downloadSharedSongAsync(songTitle, sharedLink, downloadLocalPath);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return errorMessage;
            }
            downloadLocalPath = FileManager.FormatPath(downloadLocalPath);
            string localPath = downloadLocalPath + songTitle;
            string songFile = await FileManager.findFileNameBasedOnExtensionAsync(localPath,".song");
            if (string.IsNullOrEmpty(songFile))
            {
                return "Song File not Found in " + localPath;
            }
            await addLocalSongAsync(songTitle, songFile, localPath);
            return string.Empty;
        }

        public async Task deleteSongAsync(Song song)
        {
            await Locker.unlockSongAsync(song, Saver.savedUser());
            SongList.deleteSong(song);
        }

        public async Task<bool> openSongAsync(Song song)
        {
            string errorMessage = await updateSongAsync(song);
            if (string.IsNullOrEmpty(errorMessage))
            {
                bool locked = await Locker.lockSongAsync(song, Saver.savedUser());
                if (Locker.isLockedByUser(song, Saver.savedUser()))
                {
                    openSongWithDAW(song);
                    await refreshSongStatusAsync(song);
                    return true;
                }
                return locked;
            }
            else
            {
                return false;
            }

        }

        public async Task<string> revertSongAsync(Song song)
        {
            string errorMessage = string.Empty;
            if (await Locker.unlockSongAsync(song, Saver.savedUser()))
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

        public async Task<bool> updatesAvailableAsync(Song song)
        {
            return await VersionTool.updatesAvailableForSongAsync(song);
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
