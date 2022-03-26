using App1.Models;
using App1.Models.Ports;
using App1Tests.Mock;
using System;
using System.IO;
using Xunit;
using System.Threading.Tasks;

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
            FileStream fileStream = File.Create(localPath + file);
            fileStream.Close();
            
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

            version = new VersioningMock(user1);
            locker = new Locker(version); 
        }

        public void Dispose()
        {
            if (song.LocalPath != null)
            {
                Directory.Delete(song.LocalPath, true);
            }
            if (Directory.Exists(version.versionPath + song.LocalPath))
            {
                Directory.Delete(version.versionPath + song.LocalPath, true);
            }
        }

        public User user1;
        public User user2;
        public Song song;
        public Locker locker;
        public VersioningMock version;
    }


    public class LockerTest : TestsBase
    {
        [Fact]
        public async Task LockSongTest()
        {
            //Simulate change in local workspace
            FileStream fileStream = File.Create(song.LocalPath + "audio1.wav");
            fileStream.Close();
            Assert.True(File.Exists(song.LocalPath + "audio1.wav"));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + "audio1.wav"));

            (bool,string) result = await locker.lockSongAsync(song,user1);

            Assert.True(result.Item1);
            Assert.Equal("Song Locked", result.Item2);
            Assert.Equal(SongStatus.State.locked, song.Status.state);
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.True(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + @"audio1.wav"));
        }

        [Fact]
        public async Task UnlockSongTest()
        {
            await locker.lockSongAsync(song, user1);

            Assert.Equal(SongStatus.State.locked, song.Status.state);
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.True(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));
            //Simulate change in local workspace
            FileStream fileStream = File.Create(song.LocalPath + "audio1.wav");
            fileStream.Close();
            Assert.True(File.Exists(song.LocalPath + "audio1.wav"));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + "audio1.wav"));

            bool result = await locker.unlockSongAsync(song, user1);

            Assert.True(result);
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + @"audio1.wav"));
        }

        [Fact]
        public void UpdateSongStatusTest()
        {
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.isLockedByUser(song, user1));

            File.WriteAllText(song.LocalPath + @"\.lock", user1.Username);

            locker.updateSongStatus(song);

            Assert.Equal(SongStatus.State.locked, song.Status.state);
            Assert.True(locker.isLockedByUser(song, user1));
        }

        [Fact]
        public async Task UnlockSongUnlockedTest()
        {
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));

            bool result = await locker.unlockSongAsync(song, user1);

            Assert.True(result);
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));
        }

        [Fact]
        public async Task TryUnlockSongWithDifferentUserTest()
        {
            locker.updateSongStatus(song);
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));

            (bool, string) lockResult = await locker.lockSongAsync(song, user1);
            Assert.True(lockResult.Item1);
            Assert.Equal("Song Locked", lockResult.Item2);
            Assert.Equal(SongStatus.State.locked, song.Status.state);
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.True(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));

            bool unlockResult = await locker.unlockSongAsync(song, user2);
            Assert.False(unlockResult);
            Assert.Equal(SongStatus.State.locked, song.Status.state);
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.True(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));
        }

        [Fact]
        public async Task TryLockSongAlreadyLockedTest()
        {
            locker.updateSongStatus(song);
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));

            (bool, string) lockResult = await locker.lockSongAsync(song, user1);
            Assert.True(lockResult.Item1);
            Assert.Equal("Song Locked", lockResult.Item2);
            Assert.Equal(SongStatus.State.locked, song.Status.state);
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.True(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));

            lockResult = await locker.lockSongAsync(song, user2);
            Assert.False(lockResult.Item1);
            Assert.Equal("Already Locked", lockResult.Item2);
            Assert.Equal(SongStatus.State.locked, song.Status.state);
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.True(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));
        }

        [Fact]
        public async Task TryLockSongAlreadyLockedByUserTest()
        {
            locker.updateSongStatus(song);
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));

            (bool, string) lockResult = await locker.lockSongAsync(song, user1);
            Assert.True(lockResult.Item1);
            Assert.Equal("Song Locked", lockResult.Item2);
            Assert.Equal(SongStatus.State.locked, song.Status.state);
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.True(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));

            lockResult = await locker.lockSongAsync(song, user1);
            Assert.True(lockResult.Item1);
            Assert.Equal("Song Locked", lockResult.Item2);
            Assert.Equal(SongStatus.State.locked, song.Status.state);
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.True(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));
        }

        [Fact]
        public async Task TryLockSongWithWrongBandNameCredentialTest()
        {
            locker.updateSongStatus(song);
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));

            user1.BandName = "WrongUsername";
            version = new VersioningMock(user1);
            locker = new Locker(version);

            (bool, string)  lockResult = await locker.lockSongAsync(song, user1);
            Assert.False(lockResult.Item1);
            Assert.Equal("Error Bad Credentials", lockResult.Item2);
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));
        }

        [Fact]
        public async Task TryLockSongWithWrongBandPasswordCredentialTest()
        {
            locker.updateSongStatus(song);
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));

            user1.BandPassword = "WrongPassword";
            version = new VersioningMock(user1);
            locker = new Locker(version);

            (bool, string) lockResult = await locker.lockSongAsync(song, user1);
            Assert.False(lockResult.Item1);
            Assert.Equal("Error Bad Credentials", lockResult.Item2);
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));
        }
    }
}