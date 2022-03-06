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
            (bool,string) result = await locker.lockSongAsync(song,user1);

            Assert.True(result.Item1);
            Assert.Equal("Song Locked", result.Item2);
            Assert.Equal(Song.SongStatus.locked, song.Status);
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.True(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));
        }

        [Fact]
        public void UpdateSongStatusTest()
        {
            Assert.Equal(Song.SongStatus.upToDate, song.Status);
            Assert.False(locker.isLockedByUser(song, user1));

            File.WriteAllText(song.LocalPath + @"\.lock", user1.GitUsername);

            locker.updateSongStatus(song);

            Assert.Equal(Song.SongStatus.locked, song.Status);
            Assert.True(locker.isLockedByUser(song, user1));
        }

        [Fact]
        public async Task UnlockSongTest()
        {
            await locker.lockSongAsync(song,user1);

            Assert.Equal(Song.SongStatus.locked, song.Status);
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.True(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));

            bool result = await locker.unlockSongAsync(song, user1);

            Assert.True(result);
            Assert.Equal(Song.SongStatus.upToDate, song.Status);
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));
        }

        [Fact]
        public async Task UnlockSongUnlockedTest()
        {
            Assert.Equal(Song.SongStatus.upToDate, song.Status);
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));

            bool result = await locker.unlockSongAsync(song, user1);

            Assert.True(result);
            Assert.Equal(Song.SongStatus.upToDate, song.Status);
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));
        }

        [Fact]
        public async Task TryUnlockSongWithDifferentUserTest()
        {
            locker.updateSongStatus(song);
            Assert.Equal(Song.SongStatus.upToDate, song.Status);
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));

            (bool, string) lockResult = await locker.lockSongAsync(song, user1);
            Assert.True(lockResult.Item1);
            Assert.Equal("Song Locked", lockResult.Item2);
            Assert.Equal(Song.SongStatus.locked, song.Status);
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.True(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));

            bool unlockResult = await locker.unlockSongAsync(song, user2);
            Assert.False(unlockResult);
            Assert.Equal(Song.SongStatus.locked, song.Status);
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.True(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));
        }

        [Fact]
        public async Task TryLockSongAlreadyLockedTest()
        {
            locker.updateSongStatus(song);
            Assert.Equal(Song.SongStatus.upToDate, song.Status);
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));

            (bool, string) lockResult = await locker.lockSongAsync(song, user1);
            Assert.True(lockResult.Item1);
            Assert.Equal("Song Locked", lockResult.Item2);
            Assert.Equal(Song.SongStatus.locked, song.Status);
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.True(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));

            lockResult = await locker.lockSongAsync(song, user2);
            Assert.False(lockResult.Item1);
            Assert.Equal("Already Locked", lockResult.Item2);
            Assert.Equal(Song.SongStatus.locked, song.Status);
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.True(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));
        }

        [Fact]
        public async Task TryLockSongAlreadyLockedByUserTest()
        {
            locker.updateSongStatus(song);
            Assert.Equal(Song.SongStatus.upToDate, song.Status);
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));

            (bool, string) lockResult = await locker.lockSongAsync(song, user1);
            Assert.True(lockResult.Item1);
            Assert.Equal("Song Locked", lockResult.Item2);
            Assert.Equal(Song.SongStatus.locked, song.Status);
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.True(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));

            lockResult = await locker.lockSongAsync(song, user1);
            Assert.True(lockResult.Item1);
            Assert.Equal("Song Locked", lockResult.Item2);
            Assert.Equal(Song.SongStatus.locked, song.Status);
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.True(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));
        }

        [Fact]
        public async Task TryLockSongWithWrongGitLabUsernameCredentialTest()
        {
            locker.updateSongStatus(song);
            Assert.Equal(Song.SongStatus.upToDate, song.Status);
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));

            user1.GitLabUsername = "WrongUsername";
            version = new VersioningMock(user1);
            locker = new Locker(version);

            (bool, string)  lockResult = await locker.lockSongAsync(song, user1);
            Assert.False(lockResult.Item1);
            Assert.Equal("Error Bad Credentials", lockResult.Item2);
            Assert.Equal(Song.SongStatus.upToDate, song.Status);
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));
        }

        [Fact]
        public async Task TryLockSongWithWrongGitLabPasswordCredentialTest()
        {
            locker.updateSongStatus(song);
            Assert.Equal(Song.SongStatus.upToDate, song.Status);
            Assert.False(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));

            user1.GitLabPassword = "WrongPassword";
            version = new VersioningMock(user1);
            locker = new Locker(version);

            (bool, string) lockResult = await locker.lockSongAsync(song, user1);
            Assert.False(lockResult.Item1);
            Assert.Equal("Error Bad Credentials", lockResult.Item2);
            Assert.Equal(Song.SongStatus.upToDate, song.Status);
            Assert.True(locker.isLockedByUser(song, user1));
            Assert.False(locker.isLockedByUser(song, user2));
            Assert.False(File.Exists(version.versionPath + song.LocalPath + @"\.lock"));
        }
    }
}