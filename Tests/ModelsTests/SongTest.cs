using App1.Models;
using System;
using Xunit;

namespace ModelsTests.SongTest
{
    public class SongTest
    {
        [Fact]
        public void DefaultSongCreation()
        {
            Song song = new Song();
            Assert.NotNull(song);
            Assert.Equal(string.Empty, song.Title);
            Assert.Equal(string.Empty, song.LocalPath);
            Assert.Equal(string.Empty, song.File);
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
        }

        [Fact]
        public void DefaultSongCreationWithArguments()
        {
            string expected_title = "title";
            string expected_localPath = "path";
            string expected_file = "file";
            Song song = new Song(expected_title, expected_file, expected_localPath);

            Assert.NotNull(song);
            Assert.Equal(expected_title, song.Title);
            Assert.Equal(expected_localPath, song.LocalPath);
            Assert.Equal(expected_file, song.File);
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
        }

        [Fact]
        public void TwoEqualSongsTest()
        {
            string expected_title = "title";
            string expected_localPath = "path";
            string expected_file = "file";
            string guid = Guid.NewGuid().ToString();
            Song song1 = new Song(expected_title, expected_file, expected_localPath, guid);
            Song song2 = new Song(expected_title, expected_file, expected_localPath, guid);

            Assert.Equal(song1, song2);
        }

        [Fact]
        public void TwoDifferentSongsTest()
        {
            string expected_title = "title";
            string expected_localPath = "path";
            string expected_file = "file";
            Song song1 = new Song(expected_title, expected_file, expected_localPath);
            expected_title = "title2";
            expected_localPath = "path2";
            expected_file = "file2";
            Song song2 = new Song(expected_title, expected_file, expected_localPath);

            Assert.NotEqual(song1, song2);
        }

        [Fact]
        public void OneNullSongsTest()
        {
            string expected_title = "title";
            string expected_localPath = "path";
            string expected_file = "file";
            Song song1 = new Song(expected_title, expected_file, expected_localPath);
            Song? song2 = null;

            Assert.False(song1.Equals(song2));
        }
    }
}