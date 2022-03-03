using App1.Models;
using App1.Models.Ports;
using App1Tests.Mock;
using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace App1Tests.Models
{
    public abstract class TestsBase : IDisposable
    {
        protected TestsBase()
        {
            string title = "title";
            string localPath = @".";
            string file = "file";
            song = new Song(title, file, localPath);
            //These are the Users accepted by the versionning mock to simulate connexion problems
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

            IVersionTool version = new VersioningMock(user1);
            locker = new Locker(version); 
        }

        public void Dispose()
        {
            if (File.Exists(song.LocalPath + @"\.lock"))
            {
                File.Delete(song.LocalPath + @"\.lock");
            }
        }

        public User user1;
        public User user2;
        public Song song;
        public Locker locker;
    }

    [TestClass]
    public class LockerTest : TestsBase
    {
        [TestMethod]
        public async void LockSongTest()
        {

            (bool,string) result = await locker.lockSongAsync(song,user1);

            
            Assert.IsTrue(result.Item1);
            Assert.AreEqual("Song Locked", result.Item2);
            Assert.AreEqual(Song.SongStatus.locked, song.Status);
            Assert.IsTrue(locker.isLockedByUser(song, user1));
            
        }

        [TestMethod]
        public void UpdateSongStatusTest()
        {
            Assert.AreEqual(Song.SongStatus.upToDate, song.Status);
            Assert.IsFalse(locker.isLockedByUser(song, user1));

            File.WriteAllText(song.LocalPath + @"\.lock", user1.GitUsername);

            locker.updateSongStatus(song);

            Assert.AreEqual(Song.SongStatus.locked, song.Status);
            Assert.IsTrue(locker.isLockedByUser(song, user1));
        }

        [TestMethod]
        public async void UnlockSongTest()
        {
            await locker.lockSongAsync(song,user1);

            Assert.AreEqual(Song.SongStatus.locked, song.Status);
            Assert.IsTrue(locker.isLockedByUser(song, user1));

            bool result = await locker.unlockSongAsync(song, user1);

            Assert.IsTrue(result);
            Assert.AreEqual(Song.SongStatus.upToDate, song.Status);
            Assert.IsFalse(locker.isLockedByUser(song, user1));
        }

        [TestMethod]
        public async void TryUnlockSongWithDifferentUserTest()
        {
            locker.updateSongStatus(song);
            Assert.AreEqual(Song.SongStatus.upToDate, song.Status);
            Assert.IsFalse(locker.isLockedByUser(song, user1));
            Assert.IsFalse(locker.isLockedByUser(song, user2));

            (bool, string) lockResult = await locker.lockSongAsync(song, user1);
            Assert.IsTrue(lockResult.Item1);
            Assert.AreEqual("Song Locked", lockResult.Item2);
            Assert.AreEqual(Song.SongStatus.locked, song.Status);
            Assert.IsTrue(locker.isLockedByUser(song, user1));
            Assert.IsFalse(locker.isLockedByUser(song, user2));

            bool unlockResult = await locker.unlockSongAsync(song, user2);
            Assert.IsFalse(unlockResult);
            Assert.AreEqual(Song.SongStatus.locked, song.Status);
            Assert.IsTrue(locker.isLockedByUser(song, user1));
            Assert.IsFalse(locker.isLockedByUser(song, user2));
        }

        [TestMethod]
        public async void TryLockSongAlreadyLockedTest()
        {
            locker.updateSongStatus(song);
            Assert.AreEqual(Song.SongStatus.upToDate, song.Status);
            Assert.IsFalse(locker.isLockedByUser(song, user1));
            Assert.IsFalse(locker.isLockedByUser(song, user2));

            (bool, string) lockResult = await locker.lockSongAsync(song, user1);
            Assert.IsTrue(lockResult.Item1);
            Assert.AreEqual("Song Locked", lockResult.Item2);
            Assert.AreEqual(Song.SongStatus.locked, song.Status);
            Assert.IsTrue(locker.isLockedByUser(song, user1));
            Assert.IsFalse(locker.isLockedByUser(song, user2));

            lockResult = await locker.lockSongAsync(song, user2);
            Assert.IsFalse(lockResult.Item1);
            Assert.AreEqual("Already Locked", lockResult.Item2);
            Assert.AreEqual(Song.SongStatus.locked, song.Status);
            Assert.IsTrue(locker.isLockedByUser(song, user1));
            Assert.IsFalse(locker.isLockedByUser(song, user2));
        }

        [TestMethod]
        public async void TryLockSongAlreadyLockedByUserTest()
        {
            locker.updateSongStatus(song);
            Assert.AreEqual(Song.SongStatus.upToDate, song.Status);
            Assert.IsFalse(locker.isLockedByUser(song, user1));
            Assert.IsFalse(locker.isLockedByUser(song, user2));

            (bool, string) lockResult = await locker.lockSongAsync(song, user1);
            Assert.IsTrue(lockResult.Item1);
            Assert.AreEqual("Song Locked", lockResult.Item2);
            Assert.AreEqual(Song.SongStatus.locked, song.Status);
            Assert.IsTrue(locker.isLockedByUser(song, user1));
            Assert.IsFalse(locker.isLockedByUser(song, user2));

            lockResult = await locker.lockSongAsync(song, user1);
            Assert.IsTrue(lockResult.Item1);
            Assert.AreEqual("Song Locked", lockResult.Item2);
            Assert.AreEqual(Song.SongStatus.locked, song.Status);
            Assert.IsTrue(locker.isLockedByUser(song, user1));
            Assert.IsFalse(locker.isLockedByUser(song, user2));
        }

        [TestMethod]
        public async void TryLockSongWithWrongCredentialTest()
        {
            locker.updateSongStatus(song);
            Assert.AreEqual(Song.SongStatus.upToDate, song.Status);
            Assert.IsFalse(locker.isLockedByUser(song, user1));
            Assert.IsFalse(locker.isLockedByUser(song, user2));

            (bool, string) lockResult = await locker.lockSongAsync(song, user1);
            Assert.IsTrue(lockResult.Item1);
            Assert.AreEqual("Song Locked", lockResult.Item2);
            Assert.AreEqual(Song.SongStatus.locked, song.Status);
            Assert.IsTrue(locker.isLockedByUser(song, user1));
            Assert.IsFalse(locker.isLockedByUser(song, user2));

            user1.GitLabUsername = "WrongUsername";
            IVersionTool version = new VersioningMock(user1);
            locker = new Locker(version);

            lockResult = await locker.lockSongAsync(song, user1);
            Assert.IsFalse(lockResult.Item1);
            Assert.AreEqual("Error Bad Credentials", lockResult.Item2);
            Assert.AreEqual(Song.SongStatus.locked, song.Status);
            Assert.IsTrue(locker.isLockedByUser(song, user1));
            Assert.IsFalse(locker.isLockedByUser(song, user2));
        }
    }
}