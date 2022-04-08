﻿using App1.Models;
using App1.Models.Ports;
using App1Tests.Mock;
using System.Collections.Generic;
using Xunit;

namespace ModelsTests.SaverMockTest
{
    public class SaverMockTest
    {
        [Fact]
        public void SaveSettingsTest()
        {
            string expectedBandName = "Clic22";
            string expectedBandPassword = "password123";
            string expectedUsername = "RNG2513";
            string expectedBandEmail = "hello@gmail.com";
            User expectedUser = new User(expectedBandName, expectedBandPassword, expectedUsername, expectedBandEmail);
            string expectedMusicSyncFolder = "TestFolder";

            ISaver saver = new SaverMock();
            Settings settings = new Settings();
            settings.User = expectedUser;
            settings.MusicSyncFolder = expectedMusicSyncFolder;
            saver.saveSettings(settings);
            User userSaved = saver.savedUser();

            Assert.Equal(expectedUser, userSaved);
            string musicSyncFolderSaved = saver.savedMusicSyncFolder();
            Assert.Equal(expectedMusicSyncFolder, musicSyncFolderSaved);
        }

        [Fact]
        public void SaveSongTest()
        {
            Song expectedSong = new Song();
            expectedSong.Title = "White Road";
            expectedSong.File = @"White Road.song";
            expectedSong.LocalPath = @"C:\Documents\Test\White Road";

            ISaver saver = new SaverMock();
            saver.saveSong(expectedSong);
            List<Song> Songs = saver.savedSongs();

            Assert.Contains(expectedSong, Songs);
        }
    }
}