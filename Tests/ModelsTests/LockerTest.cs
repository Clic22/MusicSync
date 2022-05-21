using App1.Models;
using App1.Models.Ports;
using App1Tests.Mock;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using WinUIApp;
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

            transport = new Mock<ITransport>();
            saver = new SaverMock();
            saver.SaveUser(user1);
            string testDirectory = @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory\LockerTest\";
            saver.SaveMusicSyncFolder(testDirectory);
            fileManager = new FileManager();
            workspace = new MusicSyncWorkspace(saver, fileManager);
            version = new Versioning(saver, fileManager, transport.Object);
            locker = new Locker(fileManager, version);
        }

        public void Dispose()
        {
            if (Directory.Exists(song.LocalPath))
            {
                Directory.Delete(song.LocalPath, true);
            }
            if (Directory.Exists(saver.SavedMusicSyncFolder()))
            {
                Directory.Delete(saver.SavedMusicSyncFolder(), true);
            }
        }

        public User user1;
        public User user2;
        public Song song;
        public Locker locker;
        public Mock<ITransport> transport;
        public SaverMock saver;
        public FileManager fileManager;
        public MusicSyncWorkspace workspace;
        public Versioning version;
    }


    public class LockerTest : TestsBase
    {
        [Fact]
        public async Task LockSongTest()
        {
            bool result = await locker.LockSongAsync(song, user1);

            Assert.True(result);
            Assert.True(locker.IsLocked(song));
            Assert.True(locker.IsLockedByUser(song, user1));
        }

        [Fact]
        public async Task UnlockSongTest()
        {
            await locker.LockSongAsync(song, user1);
            Assert.True(locker.IsLocked(song));
            Assert.True(locker.IsLockedByUser(song, user1));

            bool result = await locker.UnlockSongAsync(song, user1);

            Assert.True(result);
            Assert.False(locker.IsLocked(song));
            Assert.False(locker.IsLockedByUser(song, user1));
        }

        [Fact]
        public async Task UnlockSongUnlockedTest()
        {
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.IsLocked(song));
            Assert.False(locker.IsLockedByUser(song, user1));

            bool result = await locker.UnlockSongAsync(song, user1);

            Assert.True(result);
            Assert.False(locker.IsLocked(song));
            Assert.False(locker.IsLockedByUser(song, user1));
            
        }

        [Fact]
        public async Task TryUnlockSongWithDifferentUserTest()
        {
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.IsLocked(song));
            Assert.False(locker.IsLockedByUser(song, user1));
            Assert.False(locker.IsLockedByUser(song, user2));

            bool lockResult = await locker.LockSongAsync(song, user1);
            Assert.True(lockResult);
            Assert.True(locker.IsLockedByUser(song, user1));
            Assert.False(locker.IsLockedByUser(song, user2));

            bool unlockResult = await locker.UnlockSongAsync(song, user2);
            Assert.False(unlockResult);
            Assert.True(locker.IsLocked(song));
            Assert.True(locker.IsLockedByUser(song, user1));
            Assert.False(locker.IsLockedByUser(song, user2));
        }

        [Fact]
        public async Task TryLockSongAlreadyLockedTest()
        {
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.IsLocked(song));
            Assert.False(locker.IsLockedByUser(song, user1));
            Assert.False(locker.IsLockedByUser(song, user2));

            bool lockResult = await locker.LockSongAsync(song, user1);
            Assert.True(lockResult);
            Assert.True(locker.IsLocked(song));
            Assert.True(locker.IsLockedByUser(song, user1));
            Assert.False(locker.IsLockedByUser(song, user2));

            lockResult = await locker.LockSongAsync(song, user2);
            Assert.False(lockResult);
            Assert.True(locker.IsLocked(song));
            Assert.True(locker.IsLockedByUser(song, user1));
            Assert.False(locker.IsLockedByUser(song, user2));
        }

        [Fact]
        public async Task TryLockSongAlreadyLockedByUserTest()
        {
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.IsLocked(song));
            Assert.False(locker.IsLockedByUser(song, user1));
            Assert.False(locker.IsLockedByUser(song, user2));

            bool lockResult = await locker.LockSongAsync(song, user1);
            Assert.True(lockResult);
            Assert.True(locker.IsLocked(song));
            Assert.True(locker.IsLockedByUser(song, user1));
            Assert.False(locker.IsLockedByUser(song, user2));

            lockResult = await locker.LockSongAsync(song, user1);
            Assert.True(lockResult);
            Assert.True(locker.IsLocked(song));
            Assert.True(locker.IsLockedByUser(song, user1));
            Assert.False(locker.IsLockedByUser(song, user2));
        }

        [Fact]
        public async Task TryLockSongWithWrongCredentialTest()
        {
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.IsLocked(song));
            Assert.False(locker.IsLockedByUser(song, user1));
            Assert.False(locker.IsLockedByUser(song, user2));

            user1.BandName = "WrongUsername";
            var SongWorkspace = workspace.GetWorkspaceForSong(song);
            transport.Setup(m => m.UploadFileAsync(SongWorkspace, ".lock", "lock")).Throws(new Exception());

            await Assert.ThrowsAnyAsync<Exception>(async () => await locker.LockSongAsync(song, user1));
            Assert.False(locker.IsLocked(song));
            Assert.False(locker.IsLockedByUser(song, user1));
            Assert.False(locker.IsLockedByUser(song, user2));
            
        }

        [Fact]
        public async Task TryUnLockSongWithWrongCredentialTest()
        {
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.IsLocked(song));
            Assert.False(locker.IsLockedByUser(song, user1));
            Assert.False(locker.IsLockedByUser(song, user2));

            bool lockResult = await locker.LockSongAsync(song, user1);

            Assert.True(lockResult);
            Assert.True(locker.IsLocked(song));
            Assert.True(locker.IsLockedByUser(song, user1));
            Assert.False(locker.IsLockedByUser(song, user2));

            user1.BandPassword = "WrongPassword";
            transport.Setup(m => m.UploadFileAsync(workspace.GetWorkspaceForSong(song), ".lock", "unlock")).Throws(new Exception());

            await Assert.ThrowsAnyAsync<Exception>(async () => await locker.UnlockSongAsync(song, user1));
            Assert.False(locker.IsLocked(song));
            Assert.False(locker.IsLockedByUser(song, user1));
            Assert.False(locker.IsLockedByUser(song, user2));
            
        }

        [Fact]
        public void NobodyLockTheSongTest()
        {
            Assert.Equal(SongStatus.State.upToDate, song.Status.state);
            Assert.False(locker.IsLocked(song));
            Assert.False(locker.IsLockedByUser(song, user1));
            Assert.False(locker.IsLockedByUser(song, user2));

            string whoLocked = locker.WhoLocked(song);
            Assert.Equal(string.Empty,whoLocked);
        }
    }
}