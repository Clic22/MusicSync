using App1.Models;
using App1.Models.Ports;
using App1Tests.Mock;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WinUIApp;
using Xunit;

namespace ModelsTests.SongsManagerTest
{
    public abstract class TestsBase : IDisposable
    {
        protected TestsBase()
        {
            //This is the User accepted by the versionning mock to simulate connexion problems
            string Username = "Hear@fdjskjè_";
            string BandPassword = "12df546@";
            string BandName = "Clic5456";
            string BandEmail = "testdklsjfhg@yahoo.com";
            user = new User(BandName, BandPassword, Username, BandEmail);

            testDirectory = @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory\";
            title = "End of the Road";
            file = "file.song";
            localPath = testDirectory + @"SongsManagerTest\End of the Road\";
            expectedSong = new Song(title, file, localPath);
            Directory.CreateDirectory(localPath);
            File.Create(localPath + file).Close();

            version = new Mock<IVersionTool>(MockBehavior.Strict);
            saver = new SaverMock();
            string musicFolder = "TestFolder";
            saver.saveSettings(user, musicFolder);
            fileManager = new FileManager();
            songsManager = new SongsManager(version.Object, saver, fileManager);
            locker = new Locker(version.Object, fileManager);
        }

        public void Dispose()
        {
            if (Directory.Exists(localPath))
            {
                Directory.Delete(localPath, true);
            }
            songsManager.SongList.Clear();
        }

        public User user;
        public Song expectedSong;
        public ISaver saver;
        public FileManager fileManager;
        public Mock<IVersionTool> version;
        public SongsManager songsManager;
        
        public Locker locker;
        public string title;
        public string file;
        public string localPath;
        public string testDirectory;
    }

    public class SongsManagerTest : TestsBase
    {
        [Fact]
        public void addAndFindSongTest()
        {
            songsManager.addLocalSong(title, file, localPath);
            Song song = songsManager.findSong(title);

            Assert.Equal(expectedSong, song);
            Assert.Contains(expectedSong, saver.savedSongs());
        }

        
        [Fact]
        public void tryFindNullSongTest()
        {
            Assert.Throws<SongsManagerException>(() => songsManager.findSong(title));
        }
        
        [Fact]
        public async Task deleteSongTest()
        {
            songsManager.addLocalSong(title, file, localPath);

            await songsManager.deleteSongAsync(expectedSong);

            Assert.Throws<SongsManagerException>(() => songsManager.findSong(title));
            Assert.DoesNotContain(expectedSong, saver.savedSongs());
        }
        
        [Fact]
        public async Task deleteSongLockedbyAnotherUserTest()
        {
            version.Setup(m => m.updatesAvailableForSongAsync(expectedSong)).Returns(Task.FromResult(false));
            version.Setup(m => m.uploadSongAsync(expectedSong, ".lock", "lock")).Returns(Task.FromResult(string.Empty));

            //GIVEN
            //A song added to the songsManager, we expect the song being added to the songstorage and
            //be saved. Then we lock the song by another user, we expect to have lock file in local and version workspace.
            songsManager.addLocalSong(title, file, localPath);
            string Username = "Second User";
            string BandPassword = "12df546@";
            string BandName = "Clic5456";
            string BandEmail = "testdklsjfhg@yahoo.com";
            User user2 = new User(BandName, BandPassword, Username, BandEmail);
            await locker.lockSongAsync(expectedSong, user2);

            //WHEN we want to delete the song
            await songsManager.deleteSongAsync(expectedSong);

            //THEN we expect the song being removed from song storage and save. We expect the song to be
            //unlocked, lock file not being removed from local and version workspace.
            Assert.Throws<SongsManagerException>(() => songsManager.findSong(title));
            Assert.DoesNotContain(expectedSong, saver.savedSongs());
            version.Verify(m => m.uploadSongAsync(expectedSong, ".lock", "lock"), Times.Once());
        }
        
        [Fact]
        public async Task deleteSongLockedbyUserTest()
        {
            version.Setup(m => m.uploadSongAsync(expectedSong, ".lock", "lock")).Returns(Task.FromResult(string.Empty));
            version.Setup(m => m.uploadSongAsync(expectedSong, ".lock", "unlock")).Returns(Task.FromResult(string.Empty));

            songsManager.addLocalSong(title, file, localPath);
            await locker.lockSongAsync(expectedSong, user);

            await songsManager.deleteSongAsync(expectedSong);

            Assert.Throws<SongsManagerException>(() => songsManager.findSong(title));
            Assert.DoesNotContain(expectedSong, saver.savedSongs());
            version.Verify(m => m.uploadSongAsync(expectedSong, ".lock", "unlock"), Times.Once());
        }
        
        [Fact]
        public async Task updateSongTest()
        {
            version.Setup(m => m.updatesAvailableForSongAsync(expectedSong)).Returns(Task.FromResult(true));
            version.Setup(m => m.updateSongAsync(expectedSong)).Returns(Task.FromResult(String.Empty));

            //Add song for synchronization
            songsManager.addLocalSong(title, file, localPath);

            await songsManager.updateSongAsync(expectedSong);

            version.Verify(m => m.updateSongAsync(expectedSong), Times.Once());
        }

        
        [Fact]
        public async Task updateSongLockedTest()
        {
            version.SetupSequence(m => m.updatesAvailableForSongAsync(expectedSong)).Returns(Task.FromResult(true))
                                                                                    .Returns(Task.FromResult(false));                                                                               
            version.Setup(m => m.updateSongAsync(expectedSong)).Returns(Task.FromResult(String.Empty));
            version.Setup(m => m.uploadSongAsync(expectedSong, ".lock", "lock")).Returns(Task.FromResult(string.Empty));

            string Username = "Second User";
            string BandPassword = "12df546@";
            string BandName = "Clic5456";
            string BandEmail = "testdklsjfhg@yahoo.com";
            User user2 = new User(BandName, BandPassword, Username, BandEmail);
            await locker.lockSongAsync(expectedSong, user2);
            //Add song for synchronization
            songsManager.addLocalSong(title, file, localPath);

            await songsManager.updateSongAsync(expectedSong);

            Assert.Equal(SongStatus.State.locked, expectedSong.Status.state);
            version.Verify(m => m.updateSongAsync(expectedSong), Times.Once());
            version.Verify(m => m.updatesAvailableForSongAsync(expectedSong), Times.Exactly(2));
        }

        [Fact]
        public async Task revertSongTest()
        {
           version.Setup(m => m.revertSongAsync(expectedSong)).Returns(Task.FromResult(String.Empty));

            //Add song for synchronization
            songsManager.addLocalSong(title, file, localPath);

            await songsManager.revertSongAsync(expectedSong);

            version.Verify(m => m.revertSongAsync(expectedSong), Times.Once());
        }
        
        [Fact]
        public async Task uploadSongTest()
        {
            version.Setup(m => m.updatesAvailableForSongAsync(expectedSong)).Returns(Task.FromResult(false));
            version.Setup(m => m.newVersionNumberAsync(expectedSong, true, false, false)).Returns(Task.FromResult("1.0.0"));
            version.Setup(m => m.uploadSongAsync(expectedSong, "New Version", "No description","1.0.0")).Returns(Task.FromResult(String.Empty));
            version.Setup(m => m.uploadSongAsync(expectedSong, ".lock", "lock")).Returns(Task.FromResult(String.Empty));
            version.Setup(m => m.uploadSongAsync(expectedSong, ".lock", "unlock")).Returns(Task.FromResult(String.Empty));

            //Add song for synchronization
            songsManager.addLocalSong(title, file, localPath);
            expectedSong.Status.state = SongStatus.State.locked;

            await songsManager.uploadNewSongVersionAsync(expectedSong, "New Version", "No description", true, false, false);

            Assert.Equal(SongStatus.State.upToDate, expectedSong.Status.state);
            version.Verify(m => m.newVersionNumberAsync(expectedSong, true, false, false), Times.Once());
            version.Verify(m => m.uploadSongAsync(expectedSong, "New Version", "No description", "1.0.0"), Times.Once());
        }
        
        [Fact]
        public async Task openSongTest()
        {
            version.SetupSequence(m => m.updatesAvailableForSongAsync(expectedSong)).Returns(Task.FromResult(true))
                                                                                    .Returns(Task.FromResult(false))
                                                                                    .Returns(Task.FromResult(false));
            version.Setup(m => m.updateSongAsync(expectedSong)).Returns(Task.FromResult(String.Empty));
            version.Setup(m => m.uploadSongAsync(expectedSong, ".lock", "lock")).Returns(Task.FromResult(string.Empty));
            File.Create(localPath + file).Close();

            await songsManager.openSongAsync(expectedSong);

            Assert.Equal(SongStatus.State.locked, expectedSong.Status.state);
            version.Verify(m => m.updatesAvailableForSongAsync(expectedSong), Times.Exactly(3));
            version.Verify(m => m.updateSongAsync(expectedSong), Times.Once());
            version.Verify(m => m.uploadSongAsync(expectedSong, ".lock", "lock"), Times.Once());
        }

        [Fact]
        public async Task openSongAlreadyLockedByUserTest()
        {
            version.SetupSequence(m => m.updatesAvailableForSongAsync(expectedSong)).Returns(Task.FromResult(true))
                                                                                    .Returns(Task.FromResult(false))
                                                                                    .Returns(Task.FromResult(false));
            version.Setup(m => m.updateSongAsync(expectedSong)).Returns(Task.FromResult(String.Empty));
            version.Setup(m => m.uploadSongAsync(expectedSong, ".lock", "lock")).Returns(Task.FromResult(string.Empty));
            File.Create(localPath + file).Close();
            await locker.lockSongAsync(expectedSong, user);

            await songsManager.openSongAsync(expectedSong);

            Assert.Equal(SongStatus.State.locked, expectedSong.Status.state);
            version.Verify(m => m.updatesAvailableForSongAsync(expectedSong), Times.Exactly(3));
            version.Verify(m => m.updateSongAsync(expectedSong), Times.Once());
            version.Verify(m => m.uploadSongAsync(expectedSong, ".lock", "lock"), Times.Once());
        }

        [Fact]
        public async Task openSongErrorAtUpdateTest()
        {
            version.Setup(m => m.updatesAvailableForSongAsync(expectedSong)).Returns(Task.FromResult(true));
                                                                                   
            version.Setup(m => m.updateSongAsync(expectedSong)).Throws(new Exception());

            await Assert.ThrowsAnyAsync<Exception>(async () => await songsManager.openSongAsync(expectedSong));

            Assert.Equal(SongStatus.State.upToDate, expectedSong.Status.state);
            version.Verify(m => m.updatesAvailableForSongAsync(expectedSong), Times.Once());
            version.Verify(m => m.updateSongAsync(expectedSong), Times.Once());
            version.Verify(m => m.uploadSongAsync(expectedSong, ".lock", "lock"), Times.Never());
        }

        [Fact]
        public async Task tryOpenSongLockedTest()
        {
            version.SetupSequence(m => m.updatesAvailableForSongAsync(expectedSong)).Returns(Task.FromResult(true))
                                                                                    .Returns(Task.FromResult(false));
            version.Setup(m => m.updateSongAsync(expectedSong)).Returns(Task.FromResult(String.Empty));
            version.Setup(m => m.uploadSongAsync(expectedSong, ".lock", "lock")).Returns(Task.FromResult(string.Empty));
            string Username = "Second User";
            string BandPassword = "12df546@";
            string BandName = "Clic5456";
            string BandEmail = "testdklsjfhg@yahoo.com";
            User user2 = new User(BandName, BandPassword, Username, BandEmail);
            await locker.lockSongAsync(expectedSong, user2);

            await Assert.ThrowsAnyAsync<Exception>(async () => await songsManager.openSongAsync(expectedSong));

            Assert.Equal(SongStatus.State.locked, expectedSong.Status.state);
            version.Verify(m => m.updatesAvailableForSongAsync(expectedSong), Times.Exactly(2));
            version.Verify(m => m.updateSongAsync(expectedSong), Times.Once());
            version.Verify(m => m.uploadSongAsync(expectedSong, ".lock", "lock"), Times.Once());
        }
        
        [Fact]
        public async Task currentVersionTest()
        {
            SongVersion songVersion = new SongVersion();
            songVersion.Number = "1.0.0";
            songVersion.Author = "Oregano";
            songVersion.Description = "No Description";
            version.Setup(m => m.currentVersionAsync(expectedSong)).Returns(Task.FromResult(songVersion));

            SongVersion currentVersion = await songsManager.currentVersionAsync(expectedSong);

            string expectedVersionDescription = "No Description";
            string expectedVersionNumber = "1.0.0";
            string expectedVersionAuthor = "Oregano";
            Assert.Equal(expectedVersionDescription, currentVersion.Description);
            Assert.Equal(expectedVersionNumber, currentVersion.Number);
            Assert.Equal(expectedVersionAuthor, currentVersion.Author);
        }

        [Fact]
        public async Task versionsTest()
        {
            SongVersion songVersion = new SongVersion();
            songVersion.Number = "1.0.0";
            songVersion.Author = "Oregano";
            songVersion.Description = "No Description";
            SongVersion songVersion2 = new SongVersion();
            songVersion2.Number = "2.0.1";
            songVersion2.Author = "Aymeric Meindre";
            songVersion2.Description = "Mastering";
            List<SongVersion> songs = new List<SongVersion>();
            songs.Add(songVersion);
            songs.Add(songVersion2);
            version.Setup(m => m.versionsAsync(expectedSong)).Returns(Task.FromResult(songs));

            List<SongVersion> versions = await songsManager.versionsAsync(expectedSong);

            Assert.Contains(songVersion, versions);
            Assert.Contains(songVersion2, versions);
        }
        
        [Fact]
        public async Task addSharedSongTest()
        {
            string songTitle = "End of the Road";
            string sharedLink = "http://test.com/band/end-of-the-road";
            string downloadPath = localPath;
            Song expectedSong = new Song(songTitle, "file.song", downloadPath + songTitle + '\\');
            version.Setup(m => m.downloadSharedSongAsync(songTitle + '\\', sharedLink, downloadPath)).Returns(Task.FromResult(string.Empty))
                                                                                                     .Callback(() => { 
                                                                                                         Directory.CreateDirectory(downloadPath + songTitle + '\\');
                                                                                                         File.Create(downloadPath + songTitle + '\\' + file).Close(); 
                                                                                                     });
            version.Setup(m => m.updatesAvailableForSongAsync(expectedSong)).Returns(Task.FromResult(false));
            
            await songsManager.addSharedSongAsync(songTitle, sharedLink, downloadPath);

            //We expect a songVersioned created with the title
            Song song = songsManager.findSong(songTitle);
            Assert.Equal(expectedSong, song);
            Assert.Contains(expectedSong, saver.savedSongs());
            version.Verify(m => m.downloadSharedSongAsync(songTitle + '\\', sharedLink, downloadPath), Times.Once());
            version.Verify(m => m.updatesAvailableForSongAsync(expectedSong), Times.Once());
        }
        
        [Theory]
        [InlineData("End of the Road", "http://test.com/band/end-of-the-road", @"./SongsManagerTest\")]
        public async Task addSharedSongErrorDownloadTest(string songTitle, string sharedLink, string downloadPath)
        {
            version.Setup(m => m.downloadSharedSongAsync(songTitle + '\\', sharedLink, downloadPath)).Throws(new Exception());

            await Assert.ThrowsAnyAsync<Exception>(async () => await songsManager.addSharedSongAsync(songTitle, sharedLink, downloadPath));

            //We expect a songVersioned created with the title
            Song expectedSong = new Song(songTitle, "file.song", downloadPath + @"\" + songTitle);
            Assert.DoesNotContain(expectedSong, songsManager.SongList);
            //We expect to have called the addSharedSongAsync method in the songsManager
            version.Verify(m => m.downloadSharedSongAsync(songTitle + '\\', sharedLink, downloadPath), Times.Once());
            Assert.DoesNotContain(expectedSong, saver.savedSongs());
        }
        
        [Theory]
        [InlineData("End of the Road", "http://test.com/band/end-of-the-road", @"./SongsManagerTest\")]
        public async Task addSharedSongErrorFileNotFoundTest(string songTitle, string sharedLink, string downloadPath)
        {
            version.Setup(m => m.downloadSharedSongAsync(songTitle + '\\', sharedLink, downloadPath)).Returns(Task.FromResult(string.Empty));

            await Assert.ThrowsAnyAsync<Exception>(async () => await songsManager.addSharedSongAsync(songTitle, sharedLink, downloadPath));

            //We expect a songVersioned created with the title
            string localPath = downloadPath + songTitle + '\\';
            Song expectedSong = new Song(songTitle, "file.song", localPath);
            Assert.DoesNotContain(expectedSong, songsManager.SongList);
        }

        
        [Theory]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task getShareLinkTest(string title, string file, string localPath)
        {
            Song song = new Song(title, file, localPath);
            version.Setup(m => m.shareSongAsync(song)).Returns(Task.FromResult("https://www.gitlab.com/end-of-the-road"));
            
            string shareLink = await songsManager.shareSongAsync(song);

            //We expect to have called the addSharedSongAsync method in the songsManager
            Assert.Equal("https://www.gitlab.com/end-of-the-road", shareLink);
            version.Verify(m => m.shareSongAsync(song), Times.Once());

        }
        
        [Fact]
        public async Task refreshStatusForSongUpToDate()
        {
            version.Setup(m => m.updatesAvailableForSongAsync(expectedSong)).Returns(Task.FromResult(false));
            
            await songsManager.refreshSongStatusAsync(expectedSong);

            Assert.Equal(SongStatus.State.upToDate, expectedSong.Status.state);
        }

        [Fact]
        public async Task refreshStatusForSongLocked()
        {
            version.Setup(m => m.updatesAvailableForSongAsync(expectedSong)).Returns(Task.FromResult(false));
            version.Setup(m => m.uploadSongAsync(expectedSong, ".lock", "lock")).Returns(Task.FromResult(string.Empty));

            string Username = "Second User";
            string BandPassword = "12df546@";
            string BandName = "Clic5456";
            string BandEmail = "testdklsjfhg@yahoo.com";
            User user2 = new User(BandName, BandPassword, Username, BandEmail);
            await locker.lockSongAsync(expectedSong, user2);
            
            await songsManager.refreshSongStatusAsync(expectedSong);

            Assert.Equal(SongStatus.State.locked, expectedSong.Status.state);
        }

        [Fact]
        public async Task refreshStatusForSongUpdatesAvailable()
        {
            version.Setup(m => m.updatesAvailableForSongAsync(expectedSong)).Returns(Task.FromResult(true));
            
            await songsManager.refreshSongStatusAsync(expectedSong);

            Assert.Equal(SongStatus.State.updatesAvailable, expectedSong.Status.state);
        }
        
    }
}
