using App1.Adapters;
using App1.Models;
using App1.Models.Ports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace AdaptersTests.LocalSettingsSaverTest
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
            string GitUsername1 = "Hear@fdjskjè_";
            string GitLabPassword1 = "12df546@";
            string GitLabUsername1 = "Clic5456";
            string GitEmail1 = "testdklsjfhg@yahoo.com";
            user1 = new User(GitLabUsername1, GitLabPassword1, GitUsername1, GitEmail1); ;
            string GitUsername2 = "Lithorama52";
            string GitLabPassword2 = "15@^_usnjdfb@";
            string GitLabUsername2 = "Erratum12";
            string GitEmail2 = "erratum12@gmail.com";
            user2 = new User(GitLabUsername2, GitLabPassword2, GitUsername2, GitEmail2);
           
        }

        public void Dispose()
        {
            foreach(Song song in songsToBeSaved)
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
        public void SaveUser1Test()
        {
            saver.saveUser(user1);

            User userSaved = saver.savedUser();
            Assert.AreEqual(user1, userSaved);
        }

        [TestMethod]
        public void SaveUser2Test()
        {
            saver.saveUser(user2);

            User userSaved = saver.savedUser();
            Assert.AreEqual(user2, userSaved);
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
        public void unSaveSongTest()
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