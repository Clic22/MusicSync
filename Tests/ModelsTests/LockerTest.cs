using App1.Models;
using App1.Models.Ports;
using App1Tests.Mock;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace ModelsTests.LockerTest
{
    public abstract class TestsBase : IDisposable
    {
        protected TestsBase()
        {
            string title = "title";
            string file = "file.song";
            string localPath = @"./LockerTest/End of the Road/";
            Directory.CreateDirectory(localPath);

            song = new Song(title, file, localPath);
            //These are the Users accepted by the versionning mock to simulate connexion problems
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

            version = new Mock<IVersionTool>();
            locker = new Locker(version.Object);
        }

        public void Dispose()
        {
            if (song.LocalPath != null)
            {
                Directory.Delete(song.LocalPath, true);
            }
        }

        public User user1;
        public User user2;
        public Song song;
        public Locker locker;
        public Mock<IVersionTool> version;
    }


    public class LockerTest : TestsBase
    {
        [Fact]
        public async Task LockSongTest()
        {
            bool result = await locker.lockSongAsync(song, user1);

            Assert.True(result);
            Assert.True(locker.isLocked(song));
            Assert.True(locker.isLockedByUser(song, user1));
        }

        [Fact]
        public async Task UnlockSongTest()
        {
            await locker.lockSongAsync(song, user1);
            Assert.True(locker.isLocked(song));
            Assert.True(locker.isLockedByUser(song, user1));

            bool result = await locker.unlockSongAsync(song, user1);

            Assert.True(result);
            Assert.False(locker.isLocked(song));
            Assert.False(locker.isLockedByUser(song, user1));
        }

        [Fact]
        public async Task UnlockSongUnlockedTest()
        {
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.isLocked(song));
            Assert.False(locker.isLockedByUser(song, user1));

            bool result = await locker.unlockSongAsync(song, user1);

            Assert.True(result);
            Assert.False(locker.isLocked(song));
            Assert.False(locker.isLockedByUser(song, user1));
            
        }

        [Fact]
        public async Task TryUnlockSongWithDifferentUserTest()
        {
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.isLocked(song));
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));

            bool lockResult = await locker.lockSongAsync(song, user1);
            Assert.True(lockResult);
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));

            bool unlockResult = await locker.unlockSongAsync(song, user2);
            Assert.False(unlockResult);
            Assert.True(locker.isLocked(song));
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
        }

        [Fact]
        public async Task TryLockSongAlreadyLockedTest()
        {
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.isLocked(song));
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));

            bool lockResult = await locker.lockSongAsync(song, user1);
            Assert.True(lockResult);
            Assert.True(locker.isLocked(song));
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));

            lockResult = await locker.lockSongAsync(song, user2);
            Assert.False(lockResult);
            Assert.True(locker.isLocked(song));
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
        }

        [Fact]
        public async Task TryLockSongAlreadyLockedByUserTest()
        {
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.isLocked(song));
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));

            bool lockResult = await locker.lockSongAsync(song, user1);
            Assert.True(lockResult);
            Assert.True(locker.isLocked(song));
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));

            lockResult = await locker.lockSongAsync(song, user1);
            Assert.True(lockResult);
            Assert.True(locker.isLocked(song));
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
        }

        [Fact]
        public async Task TryLockSongWithWrongCredentialTest()
        {
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.isLocked(song));
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));

            user1.BandName = "WrongUsername";
            version.Setup(m => m.uploadSongAsync(song, ".lock", "lock")).Returns(Task.FromResult("Error Band Credentials"));

            bool lockResult = await locker.lockSongAsync(song, user1);
            Assert.False(lockResult);
            Assert.False(locker.isLocked(song));
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            
        }

        [Fact]
        public async Task TryUnLockSongWithWrongCredentialTest()
        {
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.isLocked(song));
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));

            bool lockResult = await locker.lockSongAsync(song, user1);
            Assert.True(lockResult);
            Assert.True(locker.isLocked(song));
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));

            user1.BandPassword = "WrongPassword";
            version.Setup(m => m.uploadSongAsync(song, ".lock", "unlock")).Returns(Task.FromResult("Error Band Credentials"));

            lockResult = await locker.unlockSongAsync(song, user1);
            Assert.False(lockResult);
            Assert.False(locker.isLocked(song));
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            
        }

        [Fact]
        public void nobodyLockTheSongTest()
        {
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.isLocked(song));
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));

            string whoLocked = locker.whoLocked(song);
            Assert.Equal(string.Empty,whoLocked);
        }
    }
}