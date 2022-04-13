using App1.Models;
using App1.Models.Ports;
using App1.ViewModels;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ViewModelsTests.SongsPageViewModelTest
{
    public class SongsPageViewModelTest
    {

        [Fact]
        public void initializedSongsVersionedWithExistingSongsTest()
        {
            //Setup
            string title = "title";
            string file = "file";
            string localPath = "localPath";
            Song song = new Song(title, file, localPath);
            string title2 = "title2";
            string file2 = "file2";
            string localPath2 = "localPath2";
            Song song2 = new Song(title2, file2, localPath2);
            Mock<ISaver> saverMock = new Mock<ISaver>();
            List<Song> songsList = new List<Song>();
            songsList.Add(song);
            songsList.Add(song2);
            saverMock.Setup(m => m.savedSongs()).Returns(songsList);
            SongsStorage songs = new SongsStorage(saverMock.Object);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.SongList).Returns(songs);

            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);

            SongVersioned expectedSong = new SongVersioned(title);
            SongVersioned expectedSong2 = new SongVersioned(title2);

            Assert.Contains(expectedSong, viewModel.SongsVersioned);
            Assert.Contains(expectedSong2, viewModel.SongsVersioned);
        }

        [Fact]
        public async Task refreshSongsVersionedWithExistingSongsTest()
        {
            //Setup
            string title = "title";
            string file = "file";
            string localPath = "localPath";
            SongVersion songVersion = new SongVersion();
            songVersion.Number = "1.0.0";
            songVersion.Author = "Oregano";
            songVersion.Description = "No description";
            List<SongVersion> songsVersion = new List<SongVersion>();
            songsVersion.Add(songVersion);
            Song song = new Song(title, file, localPath);
            song.Status.state = SongStatus.State.locked;
            song.Status.whoLocked = "Oregano";
            string title2 = "title2";
            string file2 = "file2";
            string localPath2 = "localPath2";
            SongVersion songVersion2 = new SongVersion();
            songVersion2.Number = "2.1.0";
            songVersion2.Author = "Aymeric Meindre";
            songVersion2.Description = "Test new mix";
            List<SongVersion> songsVersion2 = new List<SongVersion>();
            songsVersion2.Add(songVersion);
            songsVersion2.Add(songVersion2);
            Song song2 = new Song(title2, file2, localPath2);
            song2.Status.state = SongStatus.State.updatesAvailable;
            Mock<ISaver> saverMock = new Mock<ISaver>();
            List<Song> songsList = new List<Song>();
            songsList.Add(song);
            songsList.Add(song2);
            saverMock.Setup(m => m.savedSongs()).Returns(songsList);
            SongsStorage songs = new SongsStorage(saverMock.Object);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.SongList).Returns(songs);
            songsManagerMock.Setup(m => m.findSong(song.Title)).Returns(song);
            songsManagerMock.Setup(m => m.currentVersionAsync(song)).Returns(Task.FromResult(songVersion));
            songsManagerMock.Setup(m => m.versionsAsync(song)).Returns(Task.FromResult(songsVersion));
            songsManagerMock.Setup(m => m.findSong(song2.Title)).Returns(song2);
            songsManagerMock.Setup(m => m.currentVersionAsync(song2)).Returns(Task.FromResult(songVersion2));
            songsManagerMock.Setup(m => m.versionsAsync(song2)).Returns(Task.FromResult(songsVersion2));
            
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            await viewModel.refreshSongsVersionedAsync();

            SongVersioned expectedSong = new SongVersioned(title);
            App1.ViewModels.Version expectedVersion = new App1.ViewModels.Version();
            expectedVersion.Number = "1.0.0";
            Assert.Equal("Compo v1", expectedVersion.Number);
            expectedVersion.Author = "Oregano";
            expectedVersion.Description = "No description";
            expectedSong.Status = "Locked by Oregano";
            SongVersioned expectedSong2 = new SongVersioned(title2);
            App1.ViewModels.Version expectedVersion2 = new App1.ViewModels.Version();
            expectedVersion2.Number = "2.1.0";
            Assert.Equal("Compo v2 / Mix v1", expectedVersion2.Number);
            expectedVersion2.Author = "Aymeric Meindre";
            expectedVersion2.Description = "Test new mix";
            expectedSong2.Status = "Updates Available";
            List<App1.ViewModels.Version> expectedVersions = new List<App1.ViewModels.Version>();
            expectedVersions.Add(expectedVersion);
            List<App1.ViewModels.Version> expectedVersions2 = new List<App1.ViewModels.Version>();
            expectedVersions2.Add(expectedVersion2);
            expectedVersions2.Add(expectedVersion);

            Assert.Equal(expectedVersion, viewModel.SongsVersioned.First(m => m.Equals(expectedSong)).CurrentVersion);
            Assert.Equal(expectedVersion2, viewModel.SongsVersioned.First(m => m.Equals(expectedSong2)).CurrentVersion);
            Assert.Equal(expectedVersions, viewModel.SongsVersioned.First(m => m.Equals(expectedSong)).Versions);
            Assert.Equal(expectedVersions2, viewModel.SongsVersioned.First(m => m.Equals(expectedSong2)).Versions);
            Assert.Equal(expectedSong.Status, viewModel.SongsVersioned.First(m => m.Equals(expectedSong)).Status);
            Assert.Equal(expectedSong2.Status, viewModel.SongsVersioned.First(m => m.Equals(expectedSong2)).Status);
        }

        [Fact]
        public async Task refreshSongErrorTest()
        {
            //Setup
            string title = "title";
            string file = "file";
            string localPath = "localPath";
            SongVersion songVersion = new SongVersion();
            songVersion.Number = "1.0.0";
            songVersion.Author = "Oregano";
            songVersion.Description = "No description";
            List<SongVersion> songsVersion = new List<SongVersion>();
            songsVersion.Add(songVersion);
            Song song = new Song(title, file, localPath);
            song.Status.state = SongStatus.State.locked;
            song.Status.whoLocked = "Oregano";
            Mock<ISaver> saverMock = new Mock<ISaver>();
            List<Song> songsList = new List<Song>();
            songsList.Add(song);
            saverMock.Setup(m => m.savedSongs()).Returns(songsList);
            SongsStorage songs = new SongsStorage(saverMock.Object);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.SongList).Returns(songs);
            songsManagerMock.Setup(m => m.findSong(song.Title)).Returns(song);
            songsManagerMock.Setup(m => m.currentVersionAsync(song)).Throws(new Exception());
            songsManagerMock.Setup(m => m.versionsAsync(song)).Returns(Task.FromResult(songsVersion));
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);

            //Add a new song
            await Assert.ThrowsAnyAsync<Exception>(async () => await viewModel.refreshSongsVersionedAsync());

            SongVersioned expectedSong = new SongVersioned(title);
            Assert.False(viewModel.SongsVersioned.First(m => m.Equals(expectedSong)).IsRefreshingSong);
            Assert.Equal("Error", viewModel.SongsVersioned.First(m => m.Equals(expectedSong)).Status);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road")]
        [InlineData("End of the Road", "test.song", @"User/test/End of the Road")]
        public async void addLocalSongAsyncTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            SongVersion songVersion = new SongVersion("1.0.0", string.Empty, "Aymeric");
            songsManagerMock.Setup(m => m.currentVersionAsync(song)).Returns(Task.FromResult(songVersion));
            List<SongVersion> songVersions = new List<SongVersion>();
            songVersions.Add(songVersion);
            songsManagerMock.Setup(m => m.versionsAsync(song)).Returns(Task.FromResult(songVersions));

            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);

            //Add a new song
            SongVersioned songVersioned = await viewModel.addLocalSongAsync(title, file, localPath);

            //We expect a songVersioned created with the title
            SongVersioned expectedSongVersioned = new SongVersioned(title);
            Assert.Equal(expectedSongVersioned, songVersioned);
            Assert.Equal("Compo v1", songVersioned.CurrentVersion.Number);
            Assert.Equal(String.Empty, songVersioned.CurrentVersion.Description);
            Assert.Contains(expectedSongVersioned, viewModel.SongsVersioned);
            Assert.False(viewModel.IsAddingSong);
            //We expect to have called the await addLocalSongAsync method in the songsManager
            songsManagerMock.Verify(m => m.addLocalSongAsync(title, file, localPath + '\\'), Times.Once());
            Action action = async () => await  viewModel.addLocalSongAsync(title, file, localPath);
            Assert.PropertyChanged(viewModel, "IsAddingSong", action);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road")]
        [InlineData("End of the Road", "test.song", @"User/test/End of the Road")]
        public void addLocalSongAsyncErrorTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.addLocalSongAsync(title, file, localPath + '\\')).Throws(new Exception());
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);

            //Add a new song
            var exception = Assert.ThrowsAsync<Exception>(() => viewModel. addLocalSongAsync(title, file, localPath));

            //We expect a songVersioned created with the title
            SongVersioned expectedSongVersioned = new SongVersioned(title);
            Assert.DoesNotContain(expectedSongVersioned, viewModel.SongsVersioned);
            Assert.False(viewModel.IsAddingSong);
            //We expect to have called the await addLocalSongAsync method in the songsManager
            songsManagerMock.Verify(m => m.addLocalSongAsync(title, file, localPath + '\\'), Times.Once());
        }

        [Theory]
        [InlineData("title", "http://www.yoursong.com", @"./SongsManagerTest")]
        [InlineData("End of the Road", "http://www.end-of-the-road.com", @"./SongsManagerTest")]
        public async Task addSharedSongTest(string title, string link, string downloadPath)
        {
            //Setup
            Song song = new Song(title, "file.Song", downloadPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            string expectedDescription = "New Version from an User";
            string expectedNumber = "v2.3.0";
            string expectedAuthor = "Oregano";
            SongVersion songVersion = new SongVersion(expectedNumber, expectedDescription, expectedAuthor);
            songsManagerMock.Setup(m => m.currentVersionAsync(song)).Returns(Task.FromResult(songVersion));
            List<SongVersion> songVersions = new List<SongVersion>();
            songsManagerMock.Setup(m => m.versionsAsync(song)).Returns(Task.FromResult(songVersions));

            //Add a new song
            await viewModel.addSharedSongAsync(title, link, downloadPath);

            //We expect a songVersioned created with the title
            SongVersioned expectedSongVersioned = new SongVersioned(title);
            Assert.Contains(expectedSongVersioned, viewModel.SongsVersioned);
            //We expect to have called the addSharedSongAsync method in the songsManager
            songsManagerMock.Verify(m => m.addSharedSongAsync(title, link, downloadPath + '\\'), Times.Once());
            songsManagerMock.Verify(m => m.updateSongAsync(song), Times.Once());
            songsManagerMock.Verify(m => m.currentVersionAsync(song), Times.Once());
            Action action = () => viewModel.addSharedSongAsync(title, link, downloadPath);
            Assert.PropertyChanged(viewModel, "IsAddingSong", action);
        }

        [Theory]
        [InlineData("title", "ERROR", @"./SongsManagerTest")]
        public async Task addSharedSongErrorTest(string title, string link, string downloadPath)
        {
            //Setup
            Song song = new Song(title, "file.Song", downloadPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.addSharedSongAsync(title, link, downloadPath + '\\')).Throws(new Exception());
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);

            //Add a new song
            await Assert.ThrowsAnyAsync<Exception>(async () => await viewModel.addSharedSongAsync(title, link, downloadPath));

            //We expect a songVersioned created with the title
            SongVersioned expectedSongVersioned = new SongVersioned(title);
            Assert.DoesNotContain(expectedSongVersioned, viewModel.SongsVersioned);
            //We expect to have called the addSharedSongAsync method in the songsManager
            songsManagerMock.Verify(m => m.addSharedSongAsync(title, link, downloadPath + '\\'), Times.Once());
            songsManagerMock.Verify(m => m.updateSongAsync(song), Times.Never());
            songsManagerMock.Verify(m => m.currentVersionAsync(song), Times.Never());
            Action action = () => viewModel.addSharedSongAsync(title, link, downloadPath);
            Assert.PropertyChanged(viewModel, "IsAddingSong", action);
            Assert.False(viewModel.IsAddingSong);
        }

        [Fact]
        public async Task shareLinkSongTest()
        {
            //Setup
            Song song = new Song("title", "file.Song", "LocalPath");
            SongVersioned songVersioned = new SongVersioned("title");
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong("title")).Returns(song);
            string expectedShareLink = @"https://www.gitlab.com/test.git";
            songsManagerMock.Setup(m => m.shareSongAsync(song)).Returns(Task.FromResult(expectedShareLink));
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);

            //Add a new song
            string shareLink = await viewModel.shareSongAsync(songVersioned);

            Assert.Equal(expectedShareLink, shareLink);
            //We expect to have called the addSharedSongAsync method in the songsManager
            songsManagerMock.Verify(m => m.shareSongAsync(song), Times.Once());
        }

        [Theory]
        [InlineData("title")]
        public async Task shareLinkSongErrorTest(string title)
        {
            //Setup
            Song song = new Song(title, "file.Song", "LocalPath");
            SongVersioned songVersioned = new SongVersioned(title);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            songsManagerMock.Setup(m => m.shareSongAsync(song)).Throws(new Exception());
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);

            //Add a new song
            await Assert.ThrowsAnyAsync<Exception>(async () => await viewModel.shareSongAsync(songVersioned));

            Assert.Equal("Error", songVersioned.Status);     
            //We expect to have called the addSharedSongAsync method in the songsManager
            songsManagerMock.Verify(m => m.shareSongAsync(song), Times.Once());
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task updateSongAsyncTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            string expectedDescription = "New Version from an User";
            string expectedNumber = "2.3.0";
            string expectedVersionNumber = "Compo v2 / Mix v3";
            string expectedAuthor = "Oregano";
            SongVersion songVersion = new SongVersion(expectedNumber, expectedDescription, expectedAuthor);
            songsManagerMock.Setup(m => m.currentVersionAsync(song)).Returns(Task.FromResult(songVersion));
            List<SongVersion> songVersions = new List<SongVersion>();
            songsManagerMock.Setup(m => m.versionsAsync(song)).Returns(Task.FromResult(songVersions));
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            await viewModel.addLocalSongAsync(title, file, localPath);
            SongVersioned expectedSongToBeUpdated = new SongVersioned(title);

            await viewModel.updateSongAsync(expectedSongToBeUpdated);

            Assert.Equal(string.Empty, expectedSongToBeUpdated.Status);
            Assert.Equal(expectedDescription, expectedSongToBeUpdated.CurrentVersion.Description);
            Assert.Equal(expectedVersionNumber, expectedSongToBeUpdated.CurrentVersion.Number);
            Assert.Equal(expectedAuthor, expectedSongToBeUpdated.CurrentVersion.Author);
            songsManagerMock.Verify(m => m.updateSongAsync(song), Times.Once());
            songsManagerMock.Verify(m => m.currentVersionAsync(song), Times.Exactly(2));
            Action action = () => viewModel.updateSongAsync(expectedSongToBeUpdated);
            Assert.PropertyChanged(expectedSongToBeUpdated, "Status", action);
            Assert.PropertyChanged(expectedSongToBeUpdated, "IsLoading", action);
        }

        [Theory]
        [InlineData("LockedSong", "file.song", @"./SongsManagerTest/End of the Road/")]
        public async Task updateLockedSongAsyncTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            song.Status.state = SongStatus.State.locked;
            song.Status.whoLocked = "Oregano";
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            SongVersion version = new SongVersion("1.0.0", "No Description", "Aymeric");
            songsManagerMock.Setup(m => m.currentVersionAsync(song)).Returns(Task.FromResult(version));
            List<SongVersion> songVersions = new List<SongVersion>();
            songsManagerMock.Setup(m => m.versionsAsync(song)).Returns(Task.FromResult(songVersions));
            SongVersioned expectedSongToBeUpdated = new SongVersioned(title);
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            await viewModel.addLocalSongAsync(title, file, localPath);
            

            await viewModel.updateSongAsync(expectedSongToBeUpdated);

            Assert.Equal("Locked by Oregano", expectedSongToBeUpdated.Status);
            songsManagerMock.Verify(m => m.updateSongAsync(song), Times.Once());
            Action action = () => viewModel.updateSongAsync(expectedSongToBeUpdated);
            Assert.PropertyChanged(expectedSongToBeUpdated, "Status", action);
            Assert.PropertyChanged(expectedSongToBeUpdated, "IsLoading", action);
        }

        [Theory]
        [InlineData("LockedSong", "file.song", @"./SongsManagerTest/End of the Road/")]
        public async Task updateSongAsyncErrorTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            songsManagerMock.Setup(m => m.updateSongAsync(song)).Throws(new Exception());
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            SongVersion version = new SongVersion("1.0.0", "No Description", "Aymeric");
            songsManagerMock.Setup(m => m.currentVersionAsync(song)).Returns(Task.FromResult(version));
            List<SongVersion> songVersions = new List<SongVersion>();
            songsManagerMock.Setup(m => m.versionsAsync(song)).Returns(Task.FromResult(songVersions));

            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            await viewModel.addLocalSongAsync(title, file, localPath);
            SongVersioned expectedSongToBeUpdated = new SongVersioned(title);

            await Assert.ThrowsAnyAsync<Exception>(async () => await viewModel.updateSongAsync(expectedSongToBeUpdated));

            Assert.Equal("Error", expectedSongToBeUpdated.Status);
            Assert.False(expectedSongToBeUpdated.IsUpdatingSong);
            songsManagerMock.Verify(m => m.updateSongAsync(song), Times.Once());
            Action action = () => viewModel.updateSongAsync(expectedSongToBeUpdated);
            Assert.PropertyChanged(expectedSongToBeUpdated, "Status", action);
            Assert.PropertyChanged(expectedSongToBeUpdated, "IsLoading", action);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task deleteSongAsyncTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            SongVersion songVersion = new SongVersion("1.0.0", string.Empty, "Aymeric");
            songsManagerMock.Setup(m => m.currentVersionAsync(song)).Returns(Task.FromResult(songVersion));
            List<SongVersion> songVersions = new List<SongVersion>();
            songVersions.Add(songVersion);
            songsManagerMock.Setup(m => m.versionsAsync(song)).Returns(Task.FromResult(songVersions));

            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            await viewModel.addLocalSongAsync(title, file, localPath);
            SongVersioned expectedSongToBeDeleted = new SongVersioned(title);

            await viewModel.deleteSongAsync(expectedSongToBeDeleted);

            Assert.DoesNotContain(expectedSongToBeDeleted, viewModel.SongsVersioned);
            songsManagerMock.Verify(m => m.deleteSongAsync(song), Times.Once());
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task deleteSongAsyncErrorTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Throws(new Exception());
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            SongVersioned expectedSongToBeDeleted = new SongVersioned(title);

            await Assert.ThrowsAnyAsync<Exception>(async () => await viewModel.deleteSongAsync(expectedSongToBeDeleted));

            Assert.Equal("Error", expectedSongToBeDeleted.Status);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task openSongAsyncTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            songsManagerMock.Setup(m => m.openSongAsync(song)).Returns(Task.FromResult(true));
            SongVersion songVersion = new SongVersion("1.0.0", string.Empty, "Aymeric");
            songsManagerMock.Setup(m => m.currentVersionAsync(song)).Returns(Task.FromResult(songVersion));
            List<SongVersion> songVersions = new List<SongVersion>();
            songVersions.Add(songVersion);
            songsManagerMock.Setup(m => m.versionsAsync(song)).Returns(Task.FromResult(songVersions));

            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            await viewModel.addLocalSongAsync(title, file, localPath);
            SongVersioned expectedSongToBeOpened = new SongVersioned(title);

            await viewModel.openSongAsync(expectedSongToBeOpened);

            songsManagerMock.Verify(m => m.openSongAsync(song), Times.Once());
            Action action = () => viewModel.openSongAsync(expectedSongToBeOpened);
            Assert.PropertyChanged(expectedSongToBeOpened, "Status", action);
            Assert.PropertyChanged(expectedSongToBeOpened, "IsLoading", action);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task openSongAsyncErrorTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            songsManagerMock.Setup(m => m.openSongAsync(song)).Throws(new Exception());
            SongVersion songVersion = new SongVersion("1.0.0", string.Empty, "Aymeric");
            songsManagerMock.Setup(m => m.currentVersionAsync(song)).Returns(Task.FromResult(songVersion));
            List<SongVersion> songVersions = new List<SongVersion>();
            songVersions.Add(songVersion);
            songsManagerMock.Setup(m => m.versionsAsync(song)).Returns(Task.FromResult(songVersions));

            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            await viewModel.addLocalSongAsync(title, file, localPath);
            SongVersioned expectedSongToBeOpened = new SongVersioned(title);

            await Assert.ThrowsAnyAsync<Exception>(async () => await viewModel.openSongAsync(expectedSongToBeOpened));

            songsManagerMock.Verify(m => m.openSongAsync(song), Times.Once());
            Assert.Equal("Error", expectedSongToBeOpened.Status);
            Assert.False(expectedSongToBeOpened.IsOpeningSong);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task revertSongAsyncTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            SongVersion songVersion = new SongVersion("1.0.0", string.Empty, "Aymeric");
            songsManagerMock.Setup(m => m.currentVersionAsync(song)).Returns(Task.FromResult(songVersion));
            List<SongVersion> songVersions = new List<SongVersion>();
            songVersions.Add(songVersion);
            songsManagerMock.Setup(m => m.versionsAsync(song)).Returns(Task.FromResult(songVersions));

            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            await viewModel.addLocalSongAsync(title, file, localPath);
            SongVersioned expectedSongToBeReverted = new SongVersioned(title);

            await viewModel.revertSongAsync(expectedSongToBeReverted);

            songsManagerMock.Verify(m => m.revertSongAsync(song), Times.Once());
            Action action = () => viewModel.revertSongAsync(expectedSongToBeReverted);
            Assert.PropertyChanged(expectedSongToBeReverted, "Status", action);
            Assert.PropertyChanged(expectedSongToBeReverted, "IsLoading", action);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task revertSongAsyncErrorTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            songsManagerMock.Setup(m => m.revertSongAsync(song)).Throws(new Exception());
            SongVersion songVersion = new SongVersion("1.0.0", string.Empty, "Aymeric");
            songsManagerMock.Setup(m => m.currentVersionAsync(song)).Returns(Task.FromResult(songVersion));
            List<SongVersion> songVersions = new List<SongVersion>();
            songVersions.Add(songVersion);
            songsManagerMock.Setup(m => m.versionsAsync(song)).Returns(Task.FromResult(songVersions));

            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            await viewModel.addLocalSongAsync(title, file, localPath);
            SongVersioned expectedSongToBeReverted = new SongVersioned(title);

            await Assert.ThrowsAnyAsync<Exception>(async () => await viewModel.revertSongAsync(expectedSongToBeReverted));

            songsManagerMock.Verify(m => m.revertSongAsync(song), Times.Once());
            Assert.Equal("Error", expectedSongToBeReverted.Status);
            Assert.False(expectedSongToBeReverted.IsRevertingSong);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task uploadNewSongVersionAsyncTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            List<SongVersion> songVersions = new List<SongVersion>();
            SongVersion version = new SongVersion("1.0.0", "No Description", "Oregano");
            songVersions.Add(version);
            songsManagerMock.Setup(m => m.versionsAsync(song)).Returns(Task.FromResult(songVersions));
            songsManagerMock.Setup(m => m.currentVersionAsync(song)).Returns(Task.FromResult(version));
            SongVersioned expectedSongToBeUploaded = new SongVersioned(title);
            string changeTitle = "New Title";
            string changeDescritpion = "No Description";

            await viewModel.uploadNewSongVersionAsync(expectedSongToBeUploaded, changeTitle, changeDescritpion, true, false, false);

            songsManagerMock.Verify(m => m.uploadNewSongVersionAsync(song, changeTitle, changeDescritpion, true, false, false), Times.Once());
            Action action = () => viewModel.uploadNewSongVersionAsync(expectedSongToBeUploaded, changeTitle, changeDescritpion, true, false, false);
            Assert.PropertyChanged(expectedSongToBeUploaded, "Status", action);
            Assert.PropertyChanged(expectedSongToBeUploaded, "IsLoading", action);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task uploadNewSongVersionAsyncErrorTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            SongVersion songVersion = new SongVersion("1.0.0", string.Empty, "Aymeric");
            songsManagerMock.Setup(m => m.currentVersionAsync(song)).Returns(Task.FromResult(songVersion));
            List<SongVersion> songVersions = new List<SongVersion>();
            songVersions.Add(songVersion);
            songsManagerMock.Setup(m => m.versionsAsync(song)).Returns(Task.FromResult(songVersions));
            string changeTitle = "New Title";
            string changeDescritpion = "No Description";
            songsManagerMock.Setup(m => m.uploadNewSongVersionAsync(song, changeTitle, changeDescritpion, true, false, false)).Throws(new Exception());
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            SongVersioned expectedSongToBeUploaded = new SongVersioned(title);

            await Assert.ThrowsAnyAsync<Exception>(async () => await viewModel.uploadNewSongVersionAsync(expectedSongToBeUploaded, changeTitle, changeDescritpion, true, false, false));

            songsManagerMock.Verify(m => m.uploadNewSongVersionAsync(song, changeTitle, changeDescritpion, true, false, false), Times.Once());
            Assert.Equal("Error", expectedSongToBeUploaded.Status);
            Assert.False(expectedSongToBeUploaded.IsUploadingSong);
        }
    }
}