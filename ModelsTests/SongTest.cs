using App1.Models;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace App1Tests.Models
{
    [TestClass]
    public class SongTest
    {
        [TestMethod]
        public void DefaultSongCreation()
        {
            Song song = new Song();
            Assert.IsNotNull(song);
            Assert.IsNull(song.Title);
            Assert.IsNull(song.LocalPath);
            Assert.IsNull(song.File);
            Assert.AreEqual(Song.SongStatus.upToDate, song.Status);
        }

        [TestMethod]
        public void DefaultSongCreationWithArguments()
        {
            string expected_title = "title";
            string expected_localPath = "path";
            string expected_file = "file";
            Song song = new Song(expected_title, expected_file, expected_localPath );

            Assert.IsNotNull(song);
            Assert.AreEqual(expected_title, song.Title);
            Assert.AreEqual(expected_localPath, song.LocalPath);
            Assert.AreEqual(expected_file, song.File);
            Assert.AreEqual(Song.SongStatus.upToDate, song.Status);
        }

        [TestMethod]
        public void SetStatusTriggerPropertyChanged()
        {
            bool result = false;
            Song song = new Song();
            song.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Status")
                {
                    result = true;
                }
            };

            song.Status = Song.SongStatus.locked;
            Assert.AreEqual(result, true);
        }

        [TestMethod]
        public void TwoEqualSongsTest()
        {
            string expected_title = "title";
            string expected_localPath = "path";
            string expected_file = "file";
            Song song1 = new Song(expected_title, expected_file, expected_localPath);
            Song song2 = new Song(expected_title, expected_file, expected_localPath);

            Assert.AreEqual(song1, song2);
        }
    }
}