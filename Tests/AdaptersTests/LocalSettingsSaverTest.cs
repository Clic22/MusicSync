using App1.Models;
using App1.Models.Ports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using WinUIApp;

namespace WinUIAppTests.LocalSettingsSaverTest
{
    public abstract class TestsBase : IDisposable
    {
        protected TestsBase()
        {
            saver = new LocalSettingsSaver();
            songsToBeSaved = new List<Song>();
            for (int i = 0; i < 10; i++)
            {
                string expectedTitle = "title" + i;
                string expectedLocalPath = "path" + i;
                string expectedFile = "file" + i;
                Song song = new Song(expectedTitle, expectedFile, expectedLocalPath);
                songsToBeSaved.Add(song);
            }
            string Username1 = "Hear@fdjskjè_";
            string BandPassword1 = "12df546@";
            string BandName1 = "Clic5456";
            string BandEmail1 = "testdklsjfhg@yahoo.com";
            user1 = new User(BandName1, BandPassword1, Username1, BandEmail1); ;
            string Username2 = "Lithorama52";
            string BandPassword2 = "15@^_usnjdfb@";
            string BandName2 = "Erratum12";
            string BandEmail2 = "erratum12@gmail.com";
            user2 = new User(BandName2, BandPassword2, Username2, BandEmail2);

        }

        public void Dispose()
        {
            foreach (Song song in songsToBeSaved)
            {
                saver.unsaveSong(song);
            }
            string expectedTitle = "title";
            string expectedLocalPath = "path";
            string expectedFile = "file";
            Song songDefault = new Song(expectedTitle, expectedFile, expectedLocalPath);
            saver.unsaveSong(songDefault);
            songsToBeSaved.Clear();
        }

        public User user1;
        public User user2;
        public List<Song> songsToBeSaved;
        public ISaver saver;
    }

    [TestClass]
    public class LocalSettingsSaverTest : TestsBase
    {
        [TestMethod]
        public void SaveSetting1Test()
        {
            string expectedMusicSyncFolder = "TestFolder";

            saver.saveSettings(user1, expectedMusicSyncFolder);

            User userSaved = saver.savedUser();
            Assert.AreEqual(user1, userSaved);
            string musicSyncFolderSaved = saver.savedMusicSyncFolder();
            Assert.AreEqual(expectedMusicSyncFolder, musicSyncFolderSaved);
        }

        [TestMethod]
        public void SaveSetting2Test()
        {
            string expectedMusicSyncFolder = "TestFolder";

            saver.saveSettings(user2, expectedMusicSyncFolder);

            User userSaved = saver.savedUser();
            Assert.AreEqual(user2, userSaved);
            string musicSyncFolderSaved = saver.savedMusicSyncFolder();
            Assert.AreEqual(expectedMusicSyncFolder, musicSyncFolderSaved);
        }

        [TestMethod]
        public void SaveSongTest()
        {
            Song songToBeSaved = songsToBeSaved[0];
            saver.saveSong(songToBeSaved);

            List<Song> savedSongs = saver.savedSongs();

            CollectionAssert.Contains(savedSongs, songToBeSaved);
        }

        [TestMethod]
        public void SaveMultipleSongsTest()
        {
            foreach (Song song in songsToBeSaved)
            {
                saver.saveSong(song);
            }

            List<Song> savedSongs = saver.savedSongs();

            foreach (Song song in songsToBeSaved)
            {
                CollectionAssert.Contains(savedSongs, song);
            }
        }

        [TestMethod]
        public void UnsaveSongTest()
        {
            foreach (Song song in songsToBeSaved)
            {
                saver.saveSong(song);
            }

            Song songToBeUnsaved = songsToBeSaved[5];
            saver.unsaveSong(songToBeUnsaved);
            List<Song> savedSongs = saver.savedSongs();

            CollectionAssert.DoesNotContain(savedSongs, songToBeUnsaved);
        }
    }
}