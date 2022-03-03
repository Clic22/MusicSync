using App1.Models;
using App1.Models.Ports;
using App1Tests.Mock;
using System.Collections.Generic;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace App1Tests.Models
{
    [TestClass]
    public class SongStorageTest
    {
        [TestMethod]
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
            CollectionAssert.Contains(songsStorage,song);
            CollectionAssert.Contains(songsStorage,song2);

            List<Song> savedSongs = saver.savedSongs();
            CollectionAssert.Contains(savedSongs, song);
            CollectionAssert.Contains(savedSongs, song2);

            SongsStorage songsStorage2 = new SongsStorage(saver);
            CollectionAssert.Contains(songsStorage2, song);
            CollectionAssert.Contains(songsStorage2, song2);
        }

        [TestMethod]
        public void AddNewSongTest()
        {
          ISaver saver = new SaverMock();
          SongsStorage songsStorage = new SongsStorage(saver);
          string expected_title = "title";
          string expected_localPath = "path";
          string expected_file = "file";
          Song song = new Song(expected_title, expected_file, expected_localPath );
          
          songsStorage.addNewSong(song);
            CollectionAssert.Contains(songsStorage,song);
          
          List<Song> savedSongs = saver.savedSongs();
            CollectionAssert.Contains(savedSongs, song);
        }

        [TestMethod]
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
            CollectionAssert.DoesNotContain(songsStorage,song);
            List<Song> savedSongs = saver.savedSongs();
            CollectionAssert.DoesNotContain(savedSongs,song);
        }
    }
}
