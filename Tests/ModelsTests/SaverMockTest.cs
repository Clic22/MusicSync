using App1.Models;
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
            saver.SaveSettings(expectedUser, expectedMusicSyncFolder);
            User userSaved = saver.SavedUser();

            Assert.Equal(expectedUser, userSaved);
            string musicSyncFolderSaved = saver.SavedMusicSyncFolder();
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
            saver.SaveSong(expectedSong);
            List<Song> Songs = saver.SavedSongs();

            Assert.Contains(expectedSong, Songs);
        }
    }
}