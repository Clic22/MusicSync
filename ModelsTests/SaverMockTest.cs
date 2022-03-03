using App1.Models;
using App1.Models.Ports;
using App1Tests.Mock;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace App1Tests.Models
{
    [TestClass]
    public class SaverMockTest
    {
        [TestMethod]
        public void SaveUserTest()
        {
            string expectedGitLabUsername = "Clic22";
            string expectedGitLabPassword = "password123";
            string expectedGitUsername = "RNG2513";
            string expectedGitEmail = "hello@gmail.com";
            User expectedUser = new User(expectedGitLabUsername, expectedGitLabPassword, expectedGitUsername, expectedGitEmail);
            
            ISaver saver = new SaverMock();
            saver.saveUser(expectedUser);
            User userSaved = saver.savedUser();

            Assert.AreEqual(expectedUser, userSaved);
        }

        [TestMethod]
        public void SaveSongTest()
        {
            Song expectedSong = new Song();
            expectedSong.Title = "White Road";
            expectedSong.File = @"White Road.song";
            expectedSong.LocalPath = @"C:\Documents\Test\White Road";
            
            ISaver saver = new SaverMock();
            saver.saveSong(expectedSong);
            List<Song> Songs = saver.savedSongs();

            CollectionAssert.Contains(Songs,expectedSong);
        }
    }
}