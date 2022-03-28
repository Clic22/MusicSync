using System;
using Xunit;
using App1.ViewModels;
using App1.Models;
using App1.Models.Ports;
using App1Tests.Mock;
using Moq;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public void addLocalSongTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);

            //Add a new song
            SongVersioned songVersioned = viewModel.addLocalSong(title, file, localPath);

            //We expect a songVersioned created with the title
            SongVersioned expectedSongVersioned = new SongVersioned(title);
            Assert.Equal(expectedSongVersioned, songVersioned);
            Assert.Contains(expectedSongVersioned, viewModel.SongsVersioned);
            //We expect to have called the addLocalSong method in the songsManager
            songsManagerMock.Verify(m => m.addLocalSong(title, file, localPath), Times.Once());
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
            string errorMessage = await viewModel.addSharedSongAsync(title, link, downloadPath);

            //We expect a songVersioned created with the title
            SongVersioned expectedSongVersioned = new SongVersioned(title);
            Assert.Contains(expectedSongVersioned, viewModel.SongsVersioned);
            //We expect to have called the addSharedSongAsync method in the songsManager
            songsManagerMock.Verify(m => m.addSharedSongAsync(title, link, downloadPath), Times.Once());
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
            songsManagerMock.Setup(m => m.addSharedSongAsync(title,link,downloadPath)).Returns(Task.FromResult("Error"));
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);

            //Add a new song
            string errorMessage = await viewModel.addSharedSongAsync(title, link, downloadPath);

            //We expect a songVersioned created with the title
            SongVersioned expectedSongVersioned = new SongVersioned(title);
            Assert.DoesNotContain(expectedSongVersioned, viewModel.SongsVersioned);
            Assert.Equal("Error", errorMessage);
            //We expect to have called the addSharedSongAsync method in the songsManager
            songsManagerMock.Verify(m => m.addSharedSongAsync(title, link, downloadPath), Times.Once());
            songsManagerMock.Verify(m => m.updateSongAsync(song), Times.Never());
            songsManagerMock.Verify(m => m.currentVersionAsync(song), Times.Never());
            Action action = () => viewModel.addSharedSongAsync(title, link, downloadPath);
            Assert.PropertyChanged(viewModel, "IsAddingSong", action);
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
            string expectedNumber = "v2.3.0";
            string expectedAuthor = "Oregano";
            SongVersion songVersion = new SongVersion(expectedNumber, expectedDescription, expectedAuthor);
            songsManagerMock.Setup(m => m.currentVersionAsync(song)).Returns(Task.FromResult(songVersion));

            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            viewModel.addLocalSong(title, file, localPath);
            List<SongVersion> songVersions = new List<SongVersion>();
            songsManagerMock.Setup(m => m.versionsAsync(song)).Returns(Task.FromResult(songVersions));
            SongVersioned expectedSongToBeUpdated = new SongVersioned(title);

            string error = await viewModel.updateSongAsync(expectedSongToBeUpdated);

            Assert.Equal(string.Empty, expectedSongToBeUpdated.Status);
            Assert.Equal(expectedDescription, expectedSongToBeUpdated.CurrentVersion.Description);
            Assert.Equal(expectedNumber, expectedSongToBeUpdated.CurrentVersion.Number);
            Assert.Equal(expectedAuthor, expectedSongToBeUpdated.CurrentVersion.Author);
            songsManagerMock.Verify(m => m.updateSongAsync(song), Times.Once());
            songsManagerMock.Verify(m => m.currentVersionAsync(song), Times.Once());
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
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            viewModel.addLocalSong(title, file, localPath);
            SongVersion version = new SongVersion("1.0.0", "No Description", "Aymeric");
            songsManagerMock.Setup(m => m.currentVersionAsync(song)).Returns(Task.FromResult(version));
            List<SongVersion> songVersions = new List<SongVersion>();
            songsManagerMock.Setup(m => m.versionsAsync(song)).Returns(Task.FromResult(songVersions));
            SongVersioned expectedSongToBeUpdated = new SongVersioned(title);

            string error = await viewModel.updateSongAsync(expectedSongToBeUpdated);

            Assert.Equal("Locked by Oregano", expectedSongToBeUpdated.Status);
            songsManagerMock.Verify(m => m.updateSongAsync(song), Times.Once());
            Action action = () => viewModel.updateSongAsync(expectedSongToBeUpdated);
            Assert.PropertyChanged(expectedSongToBeUpdated, "Status", action);
            Assert.PropertyChanged(expectedSongToBeUpdated, "IsLoading", action);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task updateAllSongsAsyncTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Song song2 = new Song("Test", "File2", "Another Local path");
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            songsManagerMock.Setup(m => m.findSong("Test")).Returns(song2);
            SongVersion version = new SongVersion("1.0.0", "No Description", "Aymeric");
            songsManagerMock.Setup(m => m.currentVersionAsync(song)).Returns(Task.FromResult(version));
            songsManagerMock.Setup(m => m.currentVersionAsync(song2)).Returns(Task.FromResult(version));
            songsManagerMock.Setup(m => m.updateSongAsync(song)).Returns(Task.FromResult(string.Empty));
            songsManagerMock.Setup(m => m.updateSongAsync(song2)).Returns(Task.FromResult(string.Empty));
            List<SongVersion> songVersions = new List<SongVersion>();
            songsManagerMock.Setup(m => m.versionsAsync(song)).Returns(Task.FromResult(songVersions));
            songsManagerMock.Setup(m => m.versionsAsync(song2)).Returns(Task.FromResult(songVersions));
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            viewModel.addLocalSong(title, file, localPath);
            viewModel.addLocalSong("Test", "File2", "Another Local path");

            string error = await viewModel.updateAllSongsAsync();

            songsManagerMock.Verify(m => m.updateSongAsync(song), Times.Once());
            songsManagerMock.Verify(m => m.updateSongAsync(song2), Times.Once());
            Action action = () => viewModel.updateAllSongsAsync();
            Assert.PropertyChanged(viewModel.SongsVersioned.First(songVersioned => songVersioned.Title == title), "Status", action);
            Assert.PropertyChanged(viewModel.SongsVersioned.First(songVersioned => songVersioned.Title == "Test"), "Status", action);
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
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            viewModel.addLocalSong(title, file, localPath);
            SongVersioned expectedSongToBeDeleted = new SongVersioned(title);

            await viewModel.deleteSongAsync(expectedSongToBeDeleted);

            Assert.DoesNotContain(expectedSongToBeDeleted, viewModel.SongsVersioned);
            songsManagerMock.Verify(m => m.deleteSongAsync(song), Times.Once());
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
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            viewModel.addLocalSong(title, file, localPath);
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
        public async Task revertSongAsyncTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            viewModel.addLocalSong(title, file, localPath);
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
        public async Task uploadNewSongVersionAsyncTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            viewModel.addLocalSong(title, file, localPath);
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
    }
}