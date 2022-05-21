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
            expectedSong = new Song(title, file, localPath, Guid.NewGuid().ToString());
            Directory.CreateDirectory(localPath);
            File.Create(localPath + file).Close();

            transport = new Mock<ITransport>(/*MockBehavior.Strict*/);
            fileManagerMock = new Mock<IFileManager>();
            IFileManager fileManager = fileManagerMock.Object;
            saver = new SaverMock();
            saver.saveSettings(user, testDirectory);
            songsManager = new SongsManager(transport.Object, saver, fileManager);
            versioning = new Versioning(saver, fileManager, transport.Object);
            locker = new Locker(saver, fileManager, versioning);
            workspace = new MusicSyncWorkspace(saver, fileManager);
        }

        public void Dispose()
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
            songsManager.SongList.Clear();
        }

        public User user;
        public Song expectedSong;
        public ISaver saver;
        public Mock<IFileManager> fileManagerMock;
        public Mock<ITransport> transport;
        public MusicSyncWorkspace workspace;
        public SongsManager songsManager;
        public Versioning versioning;
        
        public Locker locker;
        public string title;
        public string file;
        public string localPath;
        public string testDirectory;
    }

    public class SongsManagerTest : TestsBase
    {
        private void SongLockForUser(string username)
        {
            fileManagerMock.Setup(m => m.FileExists(".lock", expectedSong.LocalPath)).Returns(true);
            fileManagerMock.Setup(m => m.ReadFile(@".lock", expectedSong.LocalPath)).Returns(username);
        }

        private void CheckSongIsUnlocked()
        {
            var songWorkspace = workspace.GetWorkspaceForSong(expectedSong);
            transport.Verify(m => m.UploadFileAsync(songWorkspace, ".lock", "unlock"), Times.Once());
            fileManagerMock.Verify(m => m.DeleteFile(".lock", expectedSong.LocalPath), Times.Once());
        }

        private void CheckSongIsLocked()
        {
            var songWorkspace = workspace.GetWorkspaceForSong(expectedSong);
            transport.Verify(m => m.UploadFileAsync(songWorkspace, ".lock", "lock"), Times.Once());
            fileManagerMock.Verify(m => m.CreateFile(".lock", expectedSong.LocalPath), Times.Once());
            fileManagerMock.Verify(m => m.SyncFile(expectedSong.LocalPath, songWorkspace, ".lock"), Times.Once());
        }

        private void CheckSongWasUncompress()
        {
            var songWorkspace = workspace.GetWorkspaceForSong(expectedSong);
            fileManagerMock.Verify(m => m.UncompressArchiveAsync(songWorkspace + expectedSong.Title + ".zip", expectedSong.LocalPath), Times.Once());
            fileManagerMock.Verify(m => m.SyncFile(songWorkspace, expectedSong.LocalPath, ".lock"), Times.Once());
        }

        [Fact]
        public async void addLocalSongAsyncTest()
        {
            await songsManager.addLocalSongAsync(title, file, localPath);
            Song song = songsManager.findSong(title);

            Assert.Equal(title, song.Title);
            Assert.Equal(localPath, song.LocalPath);
            Assert.Equal(file, song.File);
            Assert.Contains(song, songsManager.SongList);
            Assert.Contains(song, saver.savedSongs());
            transport.Verify(m => m.UploadAllFilesAsync(workspace.GetWorkspaceForSong(song), "First Upload", string.Empty), Times.Once());
            transport.Verify(m => m.Tag(workspace.GetWorkspaceForSong(song), "1.0.0"), Times.Once());

        }

        [Fact]
        public void tryFindNullSongTest()
        {
            Assert.Throws<SongsManagerException>(() => songsManager.findSong(title));
        }

        [Fact]
        public async Task deleteSongTest()
        {
            await songsManager.addLocalSongAsync(title, file, localPath);
            expectedSong = songsManager.findSong(title);

            await songsManager.deleteSongAsync(expectedSong);

            Assert.DoesNotContain(expectedSong, songsManager.SongList);
            Assert.DoesNotContain(expectedSong, saver.savedSongs());
        }

        
        [Fact]
        public async Task deleteSongLockedbyAnotherUserTest()
        {
            string Username = "Second User";
            string BandPassword = "12df546@";
            string BandName = "Clic5456";
            string BandEmail = "testdklsjfhg@yahoo.com";
            User user2 = new User(BandName, BandPassword, Username, BandEmail);
            SongLockForUser(user2.Username);

            //WHEN we want to delete the song
            await songsManager.deleteSongAsync(expectedSong);

            //THEN we expect the song being removed from song storage and save. We expect 
            //lock file not being removed from local and version workspace.
            Assert.DoesNotContain(expectedSong, saver.savedSongs());
            Assert.DoesNotContain(expectedSong, songsManager.SongList);
            transport.VerifyNoOtherCalls();
            fileManagerMock.Verify(m => m.DeleteFile(".lock", expectedSong.LocalPath), Times.Never());
        }

        [Fact]
        public async Task deleteSongLockedbyUserTest()
        {
            SongLockForUser(user.Username);

            await songsManager.deleteSongAsync(expectedSong);

            Assert.DoesNotContain(expectedSong, saver.savedSongs());
            Assert.DoesNotContain(expectedSong, songsManager.SongList);
            CheckSongIsUnlocked();
        }
        
        [Fact]
        public async Task updateSongTest()
        {
            transport.SetupSequence(m => m.UpdatesAvailbleAsync(workspace.GetWorkspaceForSong(expectedSong))).Returns(Task.FromResult(true))
                                                                                                        .Returns(Task.FromResult(false));
            string? songArchive = workspace.GetWorkspaceForSong(expectedSong) + expectedSong.Title + ".zip";
            fileManagerMock.Setup(m => m.findFileNameBasedOnExtensionAsync(workspace.GetWorkspaceForSong(expectedSong), ".zip")).Returns(Task.FromResult<string?>(songArchive));

            await songsManager.updateSongAsync(expectedSong);

            transport.Verify(m => m.DownloadLastUpdateAsync(workspace.GetWorkspaceForSong(expectedSong)), Times.Once());
            CheckSongWasUncompress();
        }

        [Fact]
        public async Task updateSongLockedTest()
        { 
            string Username = "Second User";
            string BandPassword = "12df546@";
            string BandName = "Clic5456";
            string BandEmail = "testdklsjfhg@yahoo.com";
            User user2 = new User(BandName, BandPassword, Username, BandEmail);
            SongLockForUser(user2.Username); 
            transport.SetupSequence(m => m.UpdatesAvailbleAsync(workspace.GetWorkspaceForSong(expectedSong))).Returns(Task.FromResult(true))
                                                                                                          .Returns(Task.FromResult(false));
            string? songArchive = workspace.GetWorkspaceForSong(expectedSong) + expectedSong.Title + ".zip";
            fileManagerMock.Setup(m => m.findFileNameBasedOnExtensionAsync(workspace.GetWorkspaceForSong(expectedSong), ".zip")).Returns(Task.FromResult<string?>(songArchive));

            await songsManager.updateSongAsync(expectedSong);

            transport.Verify(m => m.DownloadLastUpdateAsync(workspace.GetWorkspaceForSong(expectedSong)), Times.Once());
            CheckSongWasUncompress();
        }

        [Fact]
        public async Task revertSongTest()
        {
            string? songArchive = workspace.GetWorkspaceForSong(expectedSong) + expectedSong.Title + ".zip";
            fileManagerMock.Setup(m => m.findFileNameBasedOnExtensionAsync(workspace.GetWorkspaceForSong(expectedSong), ".zip")).Returns(Task.FromResult<string?>(songArchive));

            await songsManager.revertSongAsync(expectedSong);

            transport.Verify(m => m.RevertToLastLocalVersionAsync(workspace.GetWorkspaceForSong(expectedSong)), Times.Once());
            CheckSongWasUncompress();
        }

        [Fact]
        public async Task revertSongLockedByUserTest()
        {
            string? songArchive = workspace.GetWorkspaceForSong(expectedSong) + expectedSong.Title + ".zip";
            fileManagerMock.Setup(m => m.findFileNameBasedOnExtensionAsync(workspace.GetWorkspaceForSong(expectedSong), ".zip")).Returns(Task.FromResult<string?>(songArchive));
            SongLockForUser(user.Username);

            await songsManager.revertSongAsync(expectedSong);

            transport.Verify(m => m.RevertToLastLocalVersionAsync(workspace.GetWorkspaceForSong(expectedSong)), Times.Once());
            CheckSongWasUncompress();
            CheckSongIsUnlocked();
        }

        [Fact]
        public async Task uploadNewSongVersionTest()
        {
            SongLockForUser(user.Username);
            var title = "New Version";
            var description = "No description";
            transport.Setup(m => m.UploadAllFilesAsync(workspace.GetWorkspaceForSong(expectedSong), title, description));
            SongVersion lastLocalVersion = new SongVersion("1.0.0", "No Description", user.Username, DateOnly.FromDateTime(DateTime.Now));
            transport.Setup(m => m.LastLocalVersionAsync(workspace.GetWorkspaceForSong(expectedSong))).Returns(Task.FromResult(lastLocalVersion));

            await songsManager.uploadNewSongVersionAsync(expectedSong, title, description, true, false, false);

            transport.Verify(m => m.UploadAllFilesAsync(workspace.GetWorkspaceForSong(expectedSong), title, description), Times.Once());
            transport.Verify(m => m.Tag(workspace.GetWorkspaceForSong(expectedSong), "2.0.0"), Times.Once());
            CheckSongIsUnlocked();
        }
        
        [Fact]
        public async Task openSongTest()
        {
            transport.SetupSequence(m => m.UpdatesAvailbleAsync(workspace.GetWorkspaceForSong(expectedSong))).Returns(Task.FromResult(true))
                                                                                                          .Returns(Task.FromResult(false));
            string? songArchive = workspace.GetWorkspaceForSong(expectedSong) + expectedSong.Title + ".zip";
            fileManagerMock.Setup(m => m.findFileNameBasedOnExtensionAsync(workspace.GetWorkspaceForSong(expectedSong), ".zip")).Returns(Task.FromResult<string?>(songArchive));

            var exception = await Record.ExceptionAsync(async () => await songsManager.openSongAsync(expectedSong));

            Assert.Null(exception);
            transport.Verify(m => m.DownloadLastUpdateAsync(workspace.GetWorkspaceForSong(expectedSong)), Times.Once());
            CheckSongWasUncompress();
            CheckSongIsLocked();   
        }

        [Fact]
        public async Task openSongAlreadyLockedByUserTest()
        {
            SongLockForUser(user.Username);

            var exception = await Record.ExceptionAsync(async () => await songsManager.openSongAsync(expectedSong));

            Assert.Null(exception);
        }
        
        [Fact]
        public async Task tryOpeningSongLockedByAnotherUserTest()
        {
            string Username = "Second User";
            string BandPassword = "12df546@";
            string BandName = "Clic5456";
            string BandEmail = "testdklsjfhg@yahoo.com";
            User user2 = new User(BandName, BandPassword, Username, BandEmail);
            SongLockForUser(user2.Username);

            Exception exception = await Assert.ThrowsAnyAsync<Exception>(async () => await songsManager.openSongAsync(expectedSong));
            
            Assert.Equal(SongStatus.State.locked, expectedSong.Status.state);
            Assert.Equal(("Song locked by " + user2.Username), exception.Message);
        }
        
        [Fact]
        public async Task currentVersionTest()
        {
            SongVersion songVersion = new SongVersion();
            songVersion.Number = "1.0.0";
            songVersion.Author = "Oregano";
            songVersion.Description = "No Description";
            transport.Setup(m => m.LastLocalVersionAsync(workspace.GetWorkspaceForSong(expectedSong))).Returns(Task.FromResult(songVersion));

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
            List<SongVersion> songVersions = new List<SongVersion>();
            songVersions.Add(songVersion);
            songVersions.Add(songVersion2);
            transport.Setup(m => m.LocalVersionsAsync(workspace.GetWorkspaceForSong(expectedSong))).Returns(Task.FromResult(songVersions));

            List<SongVersion> versions = await songsManager.versionsAsync(expectedSong);

            Assert.Contains(songVersion, versions);
            Assert.Contains(songVersion2, versions);
        }
        
        [Fact]
        public async Task upcomingVersionsTest()
        {
            SongVersion upcomingSongVersion = new SongVersion();
            upcomingSongVersion.Number = "2.0.1";
            upcomingSongVersion.Author = "Aymeric Meindre";
            upcomingSongVersion.Description = "Mastering";
            upcomingSongVersion.Date = new DateOnly(2022,5,10);
            List<SongVersion> upcomingSongVersions = new List<SongVersion>();
            upcomingSongVersions.Add(upcomingSongVersion);
            transport.Setup(m => m.UpcomingVersionsAsync(workspace.GetWorkspaceForSong(expectedSong))).Returns(Task.FromResult(upcomingSongVersions));

            List<SongVersion> upcomingVersions = await songsManager.upcomingVersionsAsync(expectedSong);

            Assert.Contains(upcomingSongVersion, upcomingVersions);
        }
        
        [Fact]
        public async Task addSharedSongTest()
        {
            string songTitle = "End of the Road";
            string guid = Guid.NewGuid().ToString();
            string sharedLink = "http://gitlab.com/" + user.BandName.Replace(" ", "-") + "/" + guid + ".git";
            Song expectedSong = new Song(songTitle, file, localPath, guid);
            fileManagerMock.Setup(m => m.FormatPath(localPath + songTitle)).Returns(localPath);
            transport.Setup(m => m.Init(sharedLink, localPath));
            fileManagerMock.Setup(m => m.findFileNameBasedOnExtensionAsync(localPath, ".song")).Returns(Task.FromResult<string?>(file));
            transport.Setup(m => m.GuidFromSharedLink(sharedLink)).Returns(guid);

            await songsManager.addSharedSongAsync(songTitle, sharedLink, localPath);

            //We expect a songVersioned created with the title
            Song song = songsManager.findSong(songTitle);
            Assert.Equal(songTitle, song.Title);
            Assert.Equal(localPath, song.LocalPath);
            Assert.Equal(file, song.File);
            Assert.Contains(song, saver.savedSongs());
            Assert.Contains(song, songsManager.SongList);
        }
  
        [Fact]
        public void getShareLinkTest()
        {
            transport.Setup(m => m.ShareLink(workspace.GetWorkspaceForSong(expectedSong))).Returns("https://gitlab.com/end-of-the-road");
            
            string shareLink = songsManager.shareSong(expectedSong);

            Assert.Equal("https://gitlab.com/end-of-the-road", shareLink);
        }
        
        [Fact]
        public async Task refreshStatusForSongUpToDateTest()
        {
            transport.Setup(m => m.UpdatesAvailbleAsync(workspace.GetWorkspaceForSong(expectedSong))).Returns(Task.FromResult(false));
            
            await songsManager.refreshSongStatusAsync(expectedSong);

            Assert.Equal(SongStatus.State.upToDate, expectedSong.Status.state);
        }
        
        [Fact]
        public async Task refreshStatusForSongLockedTest()
        {
            SongLockForUser(user.Username);

            await songsManager.refreshSongStatusAsync(expectedSong);

            Assert.Equal(SongStatus.State.locked, expectedSong.Status.state);
            Assert.Equal(user.Username, expectedSong.Status.whoLocked);
        }
        
        [Fact]
        public async Task refreshStatusForSongUpdatesAvailableTest()
        {
            transport.Setup(m => m.UpdatesAvailbleAsync(workspace.GetWorkspaceForSong(expectedSong))).Returns(Task.FromResult(true));
            
            await songsManager.refreshSongStatusAsync(expectedSong);

            Assert.Equal(SongStatus.State.updatesAvailable, expectedSong.Status.state);
        }
        
        [Fact]
        public void renameSongTest()
        {
            string newTitle = "new Title";
            string newLocalPath = expectedSong.LocalPath.Replace(expectedSong.Title + '\\', "") + newTitle;
            string newFile = newTitle + ".song";
            fileManagerMock.Setup(m => m.FormatPath(expectedSong.LocalPath.Replace(expectedSong.Title + '\\', "") + newTitle)).Returns(newLocalPath);

            songsManager.renameSong(expectedSong, newTitle);

            Assert.Equal(expectedSong.Title, newTitle);
            Assert.Equal(expectedSong.LocalPath, newLocalPath);
            Assert.Equal(expectedSong.File, newFile);
            Assert.Contains(expectedSong, saver.savedSongs());
        }
      
    }
}
