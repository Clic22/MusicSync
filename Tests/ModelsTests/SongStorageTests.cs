using App1.Models;
using App1.Models.Ports;
using App1Tests.Mock;
using System.Collections.Generic;
using Xunit;

namespace ModelsTests.SongStorageTest
{
    public class SongStorageTest
    {
        [Fact]
        public void FillSongStorageWithSavedSongsTest()
        {
            ISaver saver = new SaverMock();
            SongsStorage songsStorage = new SongsStorage(saver);
            string expected_title = "title";
            string expected_localPath = "path";
            string expected_file = "file";
            Song song = new Song(expected_title, expected_file, expected_localPath);
            expected_title = "title2";
            expected_localPath = "path2";
            expected_file = "file2";
            Song song2 = new Song(expected_title, expected_file, expected_localPath);

            songsStorage.addNewSong(song);
            songsStorage.addNewSong(song2);
            Assert.Contains(song, songsStorage);
            Assert.Contains(song2, songsStorage);

            List<Song> savedSongs = saver.savedSongs();
            Assert.Contains(song, savedSongs);
            Assert.Contains(song2, savedSongs);

            SongsStorage songsStorage2 = new SongsStorage(saver);
            Assert.Contains(song, songsStorage2);
            Assert.Contains(song2, songsStorage2);
        }

        [Fact]
        public void AddNewSongTest()
        {
            ISaver saver = new SaverMock();
            SongsStorage songsStorage = new SongsStorage(saver);
            string expected_title = "title";
            string expected_localPath = "path";
            string expected_file = "file";
            Song song = new Song(expected_title, expected_file, expected_localPath);

            songsStorage.addNewSong(song);
            Assert.Contains(song, songsStorage);

            List<Song> savedSongs = saver.savedSongs();
            Assert.Contains(song, savedSongs);
        }

        [Fact]
        public void DeleteSongTest()
        {
            ISaver saver = new SaverMock();
            SongsStorage songsStorage = new SongsStorage(saver);
            string expected_title = "title";
            string expected_localPath = "path";
            string expected_file = "file";
            Song song = new Song(expected_title, expected_file, expected_localPath);

            songsStorage.addNewSong(song);

            songsStorage.deleteSong(song);
            Assert.DoesNotContain(song, songsStorage);
            List<Song> savedSongs = saver.savedSongs();
            Assert.DoesNotContain(song, savedSongs);
        }
    }
}
