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

namespace ViewModelsTests.SongsPageViewModelTest
{
    public class SongsPageViewModelTest
    {
        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public void addSongTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);

            //Add a new song
            viewModel.addSong(title, file, localPath);

            //We expect a songVersioned created with the title
            SongVersioned expectedSongVersioned = new SongVersioned(title);
            Assert.Contains(expectedSongVersioned, viewModel.SongsVersioned);
            //We expect to have called the addSong method in the songsManager
            songsManagerMock.Verify(m => m.addSong(title, file, localPath), Times.Once());
            //We expect to update the status on the songVersioned
            Action action = () => viewModel.addSong(title, file, localPath);
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
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            viewModel.addSong(title, file, localPath);
            SongVersioned expectedSongToBeUpdated = new SongVersioned(title);

            string error = await viewModel.updateSongAsync(expectedSongToBeUpdated);

            Assert.Equal("Up to Date", expectedSongToBeUpdated.Status);
            songsManagerMock.Verify(m => m.updateSongAsync(song), Times.Once());
            Action action = () => viewModel.updateSongAsync(expectedSongToBeUpdated);
            Assert.PropertyChanged(expectedSongToBeUpdated, "Status", action);
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
            viewModel.addSong(title, file, localPath);
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
            viewModel.addSong(title, file, localPath);
            viewModel.addSong("Test", "File2", "Another Local path");

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
            viewModel.addSong(title, file, localPath);
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
            viewModel.addSong(title, file, localPath);
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
            viewModel.addSong(title, file, localPath);
            SongVersioned expectedSongToBeReverted = new SongVersioned(title);

            await viewModel.revertSongAsync(expectedSongToBeReverted);

            songsManagerMock.Verify(m => m.revertSongAsync(song), Times.Once());
            Action action = () => viewModel.revertSongAsync(expectedSongToBeReverted);
            Assert.PropertyChanged(expectedSongToBeReverted, "Status", action);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task uploadNewSongVersionTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            viewModel.addSong(title, file, localPath);
            SongVersioned expectedSongToBeUploaded = new SongVersioned(title);
            string changeTitle = "New Title";
            string changeDescritpion = "No Description";

            await viewModel.uploadNewSongVersion(expectedSongToBeUploaded, changeTitle, changeDescritpion);

            songsManagerMock.Verify(m => m.uploadNewSongVersion(song, changeTitle, changeDescritpion), Times.Once());
            Action action = () => viewModel.uploadNewSongVersion(expectedSongToBeUploaded, changeTitle, changeDescritpion);
            Assert.PropertyChanged(expectedSongToBeUploaded, "Status", action);
        }
    }
}