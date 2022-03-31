using GitVersionTool;
using App1.Models.Ports;
using App1.Models;
using Xunit;
using Moq;
using System.IO;
using System.Threading.Tasks;
using WinUIApp;
using System.Net.Http;
using System.Collections.Generic;
using System;
using System.Text;

namespace GitSongVersioningTests
{
    [Collection("Serial")]
    public class GitSongVersioningTests
    {
        [Fact]
        public async Task firstSongUpload()
        {
            string testDirectory = @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory";
            string songTitle = "End of the Road";
            string songLocalPath = testDirectory + @"\" + songTitle;
            string songFile = "file.song";

            Directory.CreateDirectory(songLocalPath);
            File.CreateText(songLocalPath + @"\" + songFile).Close();
            Assert.True(File.Exists(songLocalPath + @"\" + songFile));

            User user = new User("MusicSyncTool", "HelloWorld12", "Clic", "musicsynctool@gmail.com");
            Mock<ISaver> SaverMock = new Mock<ISaver>();
            SaverMock.Setup(m => m.savedUser()).Returns(user);
            IFileManager FileManager = new FileManager();
            IVersionTool GitVersioning = new GitSongVersioning(testDirectory, SaverMock.Object, FileManager);

            string changeTitle = "Test";
            string changeDescription = "No Description";
            string versionNumber = "1.1.1";
            Song song = new Song(songTitle, songFile, songLocalPath);

            await GitVersioning.uploadSongAsync(song, changeTitle, changeDescription, versionNumber);

            string shareLink = await GitVersioning.shareSongAsync(song);

            string songFolder = @"SongDownloaded";

            string downloadDestination = testDirectory;
            await GitVersioning.downloadSharedSongAsync(songFolder, shareLink, downloadDestination);

            string expectedSongFile = downloadDestination + @"\" + songFolder + @"\" + songFile;
            Assert.True(File.Exists(expectedSongFile));

            await deleteGitlabProject();
            deleteDirectory(testDirectory);
        }

        [Theory]
        [InlineData(true, false, false, "1.0.0")]
        [InlineData(false, true, false, "0.1.0")]
        [InlineData(false, false, true, "0.0.1")]
        [InlineData(true, true, true, "1.1.1")]
        public async Task initialVersionNumberTest(bool compo, bool mix, bool mastering, string expectedVersionNumber)
        {
            string testDirectory = @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory\";

            Mock<ISaver> SaverMock = new Mock<ISaver>();
            Mock<IFileManager> FileManagerMock = new Mock<IFileManager>();
            IVersionTool GitVersioning = new GitSongVersioning(testDirectory, SaverMock.Object, FileManagerMock.Object);
            Song song = new Song();
            string versionNumber = await GitVersioning.newVersionNumberAsync(song, compo, mix, mastering);

            Assert.Equal(expectedVersionNumber, versionNumber);

            deleteDirectory(testDirectory);
        }

        [Fact]
        public async Task versionNumberTest()
        {
            string testDirectory = @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory"; 
            string songTitle = "End of the Road";
            string songLocalPath = testDirectory + @"\" + songTitle;
            string songFile = "file.song";

            Directory.CreateDirectory(songLocalPath);
            File.CreateText(songLocalPath + @"\" + songFile).Close();
            Assert.True(File.Exists(songLocalPath + @"\" + songFile));

            Song song = new Song(songTitle, songFile, songLocalPath);
            User user = new User("MusicSyncTool", "HelloWorld12", "Clic", "musicsynctool@gmail.com");
            Mock<ISaver> SaverMock = new Mock<ISaver>();
            SaverMock.Setup(m => m.savedUser()).Returns(user);
            IFileManager FileManager = new FileManager();
            IVersionTool GitVersioning = new GitSongVersioning(testDirectory, SaverMock.Object, FileManager);
            string changeTitle = "Test";
            string changeDescription = "No Description";
            string versionNumber = "1.1.1";
            await GitVersioning.uploadSongAsync(song, changeTitle, changeDescription, versionNumber);

            List<(bool compo, bool mix, bool mastering, string expectedVersionNumber)> dataToBeTested = new List<(bool, bool, bool, string )>();
            dataToBeTested.Add((true, false, false, "2.0.0"));
            dataToBeTested.Add((false, true, false, "1.2.0"));
            dataToBeTested.Add((false, false, true, "1.1.2"));
            dataToBeTested.Add((true, false, true, "2.0.1"));
            dataToBeTested.Add((false, true, true, "1.2.1"));
            dataToBeTested.Add((true, true, false, "2.1.0"));
            dataToBeTested.Add((true, true, true, "2.1.1"));


            foreach(var data in dataToBeTested)
            {
                versionNumber = await GitVersioning.newVersionNumberAsync(song, data.compo, data.mix, data.mastering);
                Assert.Equal(data.expectedVersionNumber, versionNumber);
            }

            await deleteGitlabProject();
            deleteDirectory(testDirectory);
        }

        private static async Task deleteGitlabProject()
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("DELETE"), "https://gitlab.com/api/v4/projects/MusicSyncTool%2Fend-of-the-road"))
                {
                    request.Headers.TryAddWithoutValidation("PRIVATE-TOKEN", "glpat-qwrrhK53iz4_mmSsx8h8");
                    var response = await httpClient.SendAsync(request);
                }
            }
            System.Threading.Thread.Sleep(1000);
        }

        private static void deleteDirectory(string directoryToDelete)
        {
            var directory = new DirectoryInfo(directoryToDelete) { Attributes = FileAttributes.Normal };

            foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
            {
                info.Attributes = FileAttributes.Normal;
            }

            directory.Delete(true);
            while (directory.Exists)
            {

            }
        }
    }
}