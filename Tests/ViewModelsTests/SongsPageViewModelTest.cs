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
    public abstract class TestsBase : IDisposable
    {
        protected TestsBase()
        {
            string title = "title";
            string file = "file.song";
            string localPath = @"./SongsManagerTest/End of the Road/";
            Song song = new Song(title,file,localPath);
            string title2 = "End of the Road";
            string file2 = "test.song";
            string localPath2 = @"User/test/End of the Road/";
            Song song2 = new Song(title2, file2, localPath2);
            Song song3 = new Song("Test", "File2", "Another Local path");
            string title4 = "LockedSong";
            string file4 = "file.song";
            string localPath4 = @"./SongsManagerTest/End of the Road/";
            Song song4 = new Song(title4, file4, localPath4);
            song4.Status = Song.SongStatus.locked;
            songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            songsManagerMock.Setup(m => m.findSong(title2)).Returns(song2);
            songsManagerMock.Setup(m => m.findSong("Test")).Returns(song3);
            songsManagerMock.Setup(m => m.findSong(title4)).Returns(song4);
            songsManagerMock.Setup(m => m.updateSongAsync(song)).Returns(Task.FromResult(string.Empty));
            songsManagerMock.Setup(m => m.updateSongAsync(song2)).Returns(Task.FromResult(string.Empty));
            songsManagerMock.Setup(m => m.addSong(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            expectedSongVersioned = new SongVersioned(title);
            viewModel = new SongsPageViewModel(songsManagerMock.Object);
        }

        public void Dispose()
        {

        }

        public SongVersioned expectedSongVersioned;
        public SongsPageViewModel viewModel;
        public Mock<ISongsManager> songsManagerMock;
    }

    public class SongsPageViewModelTest : TestsBase
    {
        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public void addSongTest(string title, string file, string localPath)
        {
            viewModel.addSong(title, file, localPath);
            expectedSongVersioned = viewModel.SongsVersioned.First(songVersioned => songVersioned.Title == title);

            Assert.Contains(expectedSongVersioned, viewModel.SongsVersioned);
            Assert.Equal(title, expectedSongVersioned.Title);
            songsManagerMock.Verify(m => m.addSong(title, file, localPath), Times.Once());
            
            Action action = () => viewModel.addSong(title, file, localPath);
            Assert.PropertyChanged(expectedSongVersioned, "Status", action);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task updateSongAsyncTest(string title, string file, string localPath)
        {
            viewModel.addSong(title, file, localPath);
            expectedSongVersioned = new SongVersioned(title);

            string error = await viewModel.updateSongAsync(expectedSongVersioned);

            Song song = new Song(title, file, localPath);
            songsManagerMock.Verify(m => m.updateSongAsync(song), Times.Once());
            Action action = () => viewModel.updateSongAsync(expectedSongVersioned);
            Assert.PropertyChanged(expectedSongVersioned, "Status", action);
        }

        [Theory]
        [InlineData("LockedSong", "file.song", @"./SongsManagerTest/End of the Road/")]
        public async Task updateLockedSongAsyncTest(string title, string file, string localPath)
        {
            viewModel.addSong(title, file, localPath);
            expectedSongVersioned = new SongVersioned(title);

            string error = await viewModel.updateSongAsync(expectedSongVersioned);

            Assert.Equal("Locked", expectedSongVersioned.Status);
            Song song = new Song(title, file, localPath);
            songsManagerMock.Verify(m => m.updateSongAsync(song), Times.Once());
            Action action = () => viewModel.updateSongAsync(expectedSongVersioned);
            Assert.PropertyChanged(expectedSongVersioned, "Status", action);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task updateAllSongsAsyncTest(string title, string file, string localPath)
        {
            viewModel.addSong(title, file, localPath);
            viewModel.addSong("Test", "File2", "Another Local path");
            Song song = new Song(title, file, localPath);
            Song song2 = new Song("Test", "File2", "Another Local path");

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
            viewModel.addSong(title, file, localPath);
            expectedSongVersioned = new SongVersioned(title);
            await viewModel.deleteSongAsync(expectedSongVersioned);

            Assert.DoesNotContain(expectedSongVersioned, viewModel.SongsVersioned);
            Song song = new Song(title, file, localPath);
            songsManagerMock.Verify(m => m.deleteSongAsync(song), Times.Once());
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task openSongAsyncTest(string title, string file, string localPath)
        {
            viewModel.addSong(title, file, localPath);
            expectedSongVersioned = new SongVersioned(title);
            await viewModel.openSongAsync(expectedSongVersioned);

            Song song = new Song(title, file, localPath);
            songsManagerMock.Verify(m => m.openSongAsync(song), Times.Once());
            Action action = () => viewModel.openSongAsync(expectedSongVersioned);
            Assert.PropertyChanged(expectedSongVersioned, "Status", action);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task revertSongAsyncTest(string title, string file, string localPath)
        {
            viewModel.addSong(title, file, localPath);
            expectedSongVersioned = new SongVersioned(title);
            await viewModel.revertSongAsync(expectedSongVersioned);

            Song song = new Song(title, file, localPath);
            songsManagerMock.Verify(m => m.revertSongAsync(song), Times.Once());
            Action action = () => viewModel.revertSongAsync(expectedSongVersioned);
            Assert.PropertyChanged(expectedSongVersioned, "Status", action);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task uploadNewSongVersionTest(string title, string file, string localPath)
        {
            viewModel.addSong(title, file, localPath);
            expectedSongVersioned = new SongVersioned(title);
            string changeTitle = "New Title";
            string changeDescritpion = "No Description";
            await viewModel.uploadNewSongVersion(expectedSongVersioned, changeTitle, changeDescritpion);

            Song song = new Song(title, file, localPath);
            songsManagerMock.Verify(m => m.uploadNewSongVersion(song, changeTitle, changeDescritpion), Times.Once());
            Action action = () => viewModel.uploadNewSongVersion(expectedSongVersioned, changeTitle, changeDescritpion);
            Assert.PropertyChanged(expectedSongVersioned, "Status", action);
        }
    }
}