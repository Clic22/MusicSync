using App1.Models;
using App1.Models.Ports;
using App1Tests.Mock;
using Moq;
using System;
using System.IO;
using WinUIApp;
using Xunit;

namespace ModelsTests.MusicSyncWorkspaceTest
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
            var user = new User(BandName, BandPassword, Username, BandEmail);
            musicSyncPath = @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory\MusicSyncWorkspaceTest\";

            transportMock = new Mock<ITransport>();
            fileManager = new FileManager();
            saver = new SaverMock();
            saver.SaveUser(user);
            saver.SaveMusicSyncFolder(musicSyncPath);
        }

        public void Dispose()
        {
            if (Directory.Exists(musicSyncPath))
            {
                Directory.Delete(musicSyncPath, true);
            }
        }

        public ISaver saver;
        public IFileManager fileManager;
        public Mock<ITransport> transportMock;
        public string musicSyncPath;
    }


    public class MusicSyncWorkspaceTest : TestsBase
    {
        [Fact]
        public void GetMusicSyncWorskpace()
        {

            var workspace = new MusicSyncWorkspace(saver, fileManager);
            string musicSyncFolder = workspace.MusicSyncFolder;

            string expectedMusicSyncFolder = saver.SavedMusicSyncFolder() + ".musicsync" + Path.DirectorySeparatorChar;
            Assert.Equal(expectedMusicSyncFolder, musicSyncFolder);
        }

        [Fact]
        public void CreateWorkspaceForSong()
        {
            var song = new Song("Title", "file.song", @"test\Path\");

            var workspace = new MusicSyncWorkspace(saver, fileManager);
            string songWorkspace = workspace.GetWorkspaceForSong(song);

            string expectedWorkspace = workspace.MusicSyncFolder + song.Guid + Path.DirectorySeparatorChar;
            Assert.True(fileManager.DirectoryExists(workspace.MusicSyncFolder + song.Guid));
            Assert.Equal(expectedWorkspace, songWorkspace);
        }

        [Fact]
        public void GetAlreadyCreatedWorkspaceForSong()
        {
            var song = new Song("Title", "file.song", @"test\Path\");
            Directory.CreateDirectory(musicSyncPath + song.Guid);

            var workspace = new MusicSyncWorkspace(saver, fileManager);
            string songWorkspace = workspace.GetWorkspaceForSong(song);

            string expectedWorkspace = workspace.MusicSyncFolder + song.Guid + Path.DirectorySeparatorChar;
            Assert.True(fileManager.DirectoryExists(workspace.MusicSyncFolder + song.Guid));
            Assert.Equal(expectedWorkspace, songWorkspace);
        }

        [Fact]
        public void CreateSpecificWorkspace()
        {
            string guid = Guid.NewGuid().ToString();

            var musicSync = new MusicSyncWorkspace(saver, fileManager);
            string specificWorkspace = musicSync.GetWorkspace(guid);

            string expectedWorkspace = musicSync.MusicSyncFolder + guid + Path.DirectorySeparatorChar;
            Assert.True(fileManager.DirectoryExists(musicSync.MusicSyncFolder + guid));
            Assert.Equal(expectedWorkspace, specificWorkspace);
        }

        [Fact]
        public void GetAlreadyCreatedWorkspaceForSpecificWorkspace()
        {
            string guid = Guid.NewGuid().ToString();
            Directory.CreateDirectory(musicSyncPath + guid);

            var musicSync = new MusicSyncWorkspace(saver, fileManager);
            string specificWorkspace = musicSync.GetWorkspace(guid);

            string expectedWorkspace = musicSync.MusicSyncFolder + guid + Path.DirectorySeparatorChar;
            Assert.True(fileManager.DirectoryExists(musicSync.MusicSyncFolder + guid));
            Assert.Equal(expectedWorkspace, specificWorkspace); ;
        }

    }
}