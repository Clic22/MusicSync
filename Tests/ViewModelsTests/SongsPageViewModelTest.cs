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
        public async Task addSongTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);

            //Add a new song
            await viewModel.addSongAsync(title, file, localPath);

            //We expect a songVersioned created with the title
            SongVersioned expectedSongVersioned = new SongVersioned(title);
            Assert.Contains(expectedSongVersioned, viewModel.SongsVersioned);
            //We expect to have called the addSong method in the songsManager
            songsManagerMock.Verify(m => m.addSong(title, file, localPath), Times.Once());
            //We expect to update the status on the songVersioned
            Action action = () => viewModel.addSongAsync(title, file, localPath);
            Assert.PropertyChanged(viewModel.SongsVersioned.First(songVersioned => songVersioned.Title == title), "Status", action);
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
            songsManagerMock.Setup(m => m.versionDescriptionAsync(song)).Returns(Task.FromResult(expectedDescription));
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            await viewModel.addSongAsync(title, file, localPath);
            SongVersioned expectedSongToBeUpdated = new SongVersioned(title);

            string error = await viewModel.updateSongAsync(expectedSongToBeUpdated);

            Assert.Equal("Up to Date", expectedSongToBeUpdated.Status);
            Assert.Equal(expectedDescription, expectedSongToBeUpdated.VersionDescription);
            songsManagerMock.Verify(m => m.updateSongAsync(song), Times.Once());
            Action action = () => viewModel.updateSongAsync(expectedSongToBeUpdated);
            Assert.PropertyChanged(expectedSongToBeUpdated, "Status", action);
            Assert.PropertyChanged(expectedSongToBeUpdated, "VersionDescription", action);
            Assert.PropertyChanged(expectedSongToBeUpdated, "VersionNumber", action);
        }

        [Theory]
        [InlineData("LockedSong", "file.song", @"./SongsManagerTest/End of the Road/")]
        public async Task updateLockedSongAsyncTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            song.Status = Song.SongStatus.locked;
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            await viewModel.addSongAsync(title, file, localPath);
            SongVersioned expectedSongToBeUpdated = new SongVersioned(title);

            string error = await viewModel.updateSongAsync(expectedSongToBeUpdated);

            Assert.Equal("Locked", expectedSongToBeUpdated.Status);
            songsManagerMock.Verify(m => m.updateSongAsync(song), Times.Once());
            Action action = () => viewModel.updateSongAsync(expectedSongToBeUpdated);
            Assert.PropertyChanged(expectedSongToBeUpdated, "Status", action);
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
            songsManagerMock.Setup(m => m.updateSongAsync(song)).Returns(Task.FromResult(string.Empty));
            songsManagerMock.Setup(m => m.updateSongAsync(song2)).Returns(Task.FromResult(string.Empty));
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            await viewModel.addSongAsync(title, file, localPath);
            await viewModel.addSongAsync("Test", "File2", "Another Local path");

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
            await viewModel.addSongAsync(title, file, localPath);
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
            await viewModel.addSongAsync(title, file, localPath);
            SongVersioned expectedSongToBeOpened = new SongVersioned(title);

            await viewModel.openSongAsync(expectedSongToBeOpened);

            songsManagerMock.Verify(m => m.openSongAsync(song), Times.Once());
            Action action = () => viewModel.openSongAsync(expectedSongToBeOpened);
            Assert.PropertyChanged(expectedSongToBeOpened, "Status", action);
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
            await viewModel.addSongAsync(title, file, localPath);
            SongVersioned expectedSongToBeReverted = new SongVersioned(title);

            await viewModel.revertSongAsync(expectedSongToBeReverted);

            songsManagerMock.Verify(m => m.revertSongAsync(song), Times.Once());
            Action action = () => viewModel.revertSongAsync(expectedSongToBeReverted);
            Assert.PropertyChanged(expectedSongToBeReverted, "Status", action);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/", "", true, false, false, "v1.0.0")]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/", "v3.3.3", true, false, false, "v4.0.0")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/", "v3.3.3", false, true, false, "v3.4.0")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/", "v3.3.3", false, false, true, "v3.3.4")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/", "v3.3.3", true, true, true, "v4.1.1")]
        public async Task uploadNewSongVersionAsyncTest(string title, string file, string localPath, string initialVersionNumber, bool compo, bool mix, bool mastering, string expectedVersionNumber)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            await viewModel.addSongAsync(title, file, localPath);
            SongVersioned expectedSongToBeUploaded = new SongVersioned(title);
            string changeTitle = "New Title";
            string changeDescritpion = "No Description";

            await viewModel.uploadNewSongVersionAsync(expectedSongToBeUploaded, changeTitle, changeDescritpion, compo, mix, mastering);

            //Assert.Equal(expectedVersionNumber, expectedSongToBeUploaded.VersionNumber);
            songsManagerMock.Verify(m => m.uploadNewSongVersionAsync(song, changeTitle, changeDescritpion, compo, mix, mastering), Times.Once());
            Action action = () => viewModel.uploadNewSongVersionAsync(expectedSongToBeUploaded, changeTitle, changeDescritpion, compo, mix, mastering);
            Assert.PropertyChanged(expectedSongToBeUploaded, "Status", action);
        }
    }
}