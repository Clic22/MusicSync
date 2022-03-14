using System;
using Xunit;
using App1.ViewModels;
using App1.Models;
using App1.Models.Ports;
using App1Tests.Mock;
using Moq;
using System.IO;
using System.Linq;

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
            Directory.CreateDirectory(localPath);
            FileStream fileStream = File.Create(localPath + file);
            fileStream.Close();
            expectedSongVersioned = new SongVersioned(title);
            expectedSongVersioned.Status = "Up to Date";
            IVersionTool version = new VersioningMock(user);
            ISaver saver = new SaverMock();
            saver.saveUser(user);

            songsManagerMock = new Mock<ISongsManager>() ;
            SongsStorage songs = new SongsStorage(saver);
            songsManagerMock.SetupGet<SongsStorage>(m => m.SongList).Returns(songs);
            songsManagerMock.Setup(m => m.findSong("title")).Returns(song);
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
    }
}