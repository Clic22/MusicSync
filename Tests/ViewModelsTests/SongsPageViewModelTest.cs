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
            //This is the User accepted by the versionning mock to simulate connexion problems
            string GitUsername = "Hear@fdjskjè_";
            string GitLabPassword = "12df546@";
            string GitLabUsername = "Clic5456";
            string GitEmail = "testdklsjfhg@yahoo.com";
            user = new User(GitLabUsername, GitLabPassword, GitUsername, GitEmail);

            title = "title";
            file = "file.song";
            localPath = @"./SongsManagerTest/End of the Road/";
            Song song = new Song(title,file,localPath);
            title2 = "title2";
            file2 = "file2.song";
            localPath2 = @"./SongsManagerTest/End of the Road2/";
            Song song2 = new Song(title2, file2, localPath2);

            Directory.CreateDirectory(localPath);
            FileStream fileStream = File.Create(localPath + file);
            fileStream.Close();
            expectedSongVersioned = new SongVersioned(title);
            expectedSongVersioned.Status = "Up to Date";
            IVersionTool version = new VersioningMock(user);
            ISaver saver = new SaverMock();
            saver.saveUser(user);
            SongsStorage songs = new SongsStorage(saver);

            songsManagerMock = new Mock<ISongsManager>();
            
            songsManagerMock.SetupGet<SongsStorage>(m => m.SongList).Returns(songs);
            songsManagerMock.Setup(m => m.findSong(title)).Returns(song);
            songsManagerMock.Setup(m => m.findSong(title2)).Returns(song2);
            songsManagerMock.Setup(m => m.updateSongAsync(song)).Returns(Task.FromResult(string.Empty));
            songsManagerMock.Setup(m => m.updateSongAsync(song2)).Returns(Task.FromResult(string.Empty));
            songsManagerMock.Setup(m => m.addSong(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();


            viewModel = new SongsPageViewModel(songsManagerMock.Object);
        }

        public void Dispose()
        {

        }

        public User user;
        public SongVersioned expectedSongVersioned;
        public SongsPageViewModel viewModel;
        public string title;
        public string file;
        public string localPath;
        public string title2;
        public string file2;
        public string localPath2;
        public Mock<ISongsManager> songsManagerMock;
    }

    public class SongsPageViewModelTest : TestsBase
    {
        [Fact]
        public void addSongTest()
        {
            viewModel.addSong(title, file, localPath);
            
            Assert.Contains(expectedSongVersioned, viewModel.SongsVersioned);
            songsManagerMock.Verify(m => m.addSong(title, file, localPath), Times.Once());
            
            Action action = () => viewModel.addSong(title, file, localPath);
            Assert.PropertyChanged(viewModel.SongsVersioned.First(songVersioned => songVersioned.Title == title), "Status", action);
        }

        [Fact]
        public async Task updateSongAsyncTest()
        {
            viewModel.addSong(title, file, localPath);
            string error = await viewModel.updateSongAsync(expectedSongVersioned);

            Song song = new Song(title, file, localPath);
            songsManagerMock.Verify(m => m.updateSongAsync(song), Times.Once());
            
            Action action = () => viewModel.updateSongAsync(expectedSongVersioned);
            Assert.PropertyChanged(viewModel.SongsVersioned.First(songVersioned => songVersioned.Title == title), "Status", action);
        }

        [Fact]
        public async Task updateAllSongsAsyncTest()
        {
            viewModel.addSong(title, file, localPath);
            viewModel.addSong(title2, file2, localPath2);
            Song song = new Song(title, file, localPath);
            Song song2 = new Song(title2, file2, localPath2);

            string error = await viewModel.updateAllSongsAsync();

            songsManagerMock.Verify(m => m.updateSongAsync(song), Times.Once());
            songsManagerMock.Verify(m => m.updateSongAsync(song2), Times.Once());
            Action action = () => viewModel.updateAllSongsAsync();
            Assert.PropertyChanged(viewModel.SongsVersioned.First(songVersioned => songVersioned.Title == title), "Status", action);
            Assert.PropertyChanged(viewModel.SongsVersioned.First(songVersioned => songVersioned.Title == title2), "Status", action);
        }

        [Fact]
        public async Task deleteSongAsyncTest()
        {
            viewModel.addSong(title, file, localPath);
            await viewModel.deleteSongAsync(expectedSongVersioned);

            Assert.DoesNotContain(expectedSongVersioned, viewModel.SongsVersioned);
            Song song = new Song(title, file, localPath);
            songsManagerMock.Verify(m => m.deleteSongAsync(song), Times.Once());
        }

        [Fact]
        public async Task openSongAsyncTest()
        {
            viewModel.addSong(title, file, localPath);
            await viewModel.openSongAsync(expectedSongVersioned);

            Song song = new Song(title, file, localPath);
            songsManagerMock.Verify(m => m.openSongAsync(song), Times.Once());
            Action action = () => viewModel.openSongAsync(expectedSongVersioned);
            Assert.PropertyChanged(viewModel.SongsVersioned.First(songVersioned => songVersioned.Title == title), "Status", action);
        }

        [Fact]
        public async Task revertSongAsyncTest()
        {
            viewModel.addSong(title, file, localPath);
            await viewModel.revertSongAsync(expectedSongVersioned);

            Song song = new Song(title, file, localPath);
            songsManagerMock.Verify(m => m.revertSongAsync(song), Times.Once());
            Action action = () => viewModel.revertSongAsync(expectedSongVersioned);
            Assert.PropertyChanged(viewModel.SongsVersioned.First(songVersioned => songVersioned.Title == title), "Status", action);
        }

        [Fact]
        public async Task uploadNewSongVersionTest()
        {
            viewModel.addSong(title, file, localPath);
            string changeTitle = "New Title";
            string changeDescritpion = "No Description";
            await viewModel.uploadNewSongVersion(expectedSongVersioned, changeTitle, changeDescritpion);

            Song song = new Song(title, file, localPath);
            songsManagerMock.Verify(m => m.uploadNewSongVersion(song, changeTitle, changeDescritpion), Times.Once());
            Action action = () => viewModel.uploadNewSongVersion(expectedSongVersioned, changeTitle, changeDescritpion);
            Assert.PropertyChanged(viewModel.SongsVersioned.First(songVersioned => songVersioned.Title == title), "Status", action);
        }
    }
}