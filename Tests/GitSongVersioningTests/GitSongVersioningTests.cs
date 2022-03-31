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
using System.Net.Http.Headers;

namespace GitSongVersioningTests
{
    public abstract class TestsBase : IAsyncLifetime
    {
        protected TestsBase()
        {
            testDirectory = @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory";
            songTitle = "End of the Road";
            songLocalPath = testDirectory + @"\" + songTitle;
            songFile = "file.song";
            song = new Song(songTitle, songFile, songLocalPath);

            Directory.CreateDirectory(songLocalPath);
            File.CreateText(songLocalPath + @"\" + songFile).Close();
            Assert.True(File.Exists(songLocalPath + @"\" + songFile));

            user = new User("MusicSyncTool", "HelloWorld12", "Clic", "musicsynctool@gmail.com");
            SaverMock = new Mock<ISaver>();
            SaverMock.Setup(m => m.savedUser()).Returns(user);
            FileManager = new FileManager();
            GitVersioning = new GitSongVersioning(testDirectory, SaverMock.Object, FileManager);
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask; 
        }

        public async Task DisposeAsync()
        {
            deleteDirectory(testDirectory);
            await deleteGitlabProject();
        }

        public string testDirectory;
        public string songTitle;
        public string songLocalPath;
        public string songFile;

        public User user;
        public Song song;
        public Mock<ISaver> SaverMock;
        public IFileManager FileManager;
        public IVersionTool GitVersioning;

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
            System.Threading.Thread.Sleep(2000);
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

    [Collection("Serial")]
    public class GitSongVersioningTests : TestsBase
    {
        [Fact]
        public async Task firstSongUploadAndDownloadSong()
        {
            string changeTitle = "Test";
            string changeDescription = "No Description";
            string versionNumber = "1.1.1";

            string mediaFolder = song.LocalPath + @"\Media";
            string mediaFile = "guitar.wav";
            Directory.CreateDirectory(mediaFolder);
            File.CreateText(mediaFolder + @"\" + mediaFile).Close();
            string MelodyneFolder = song.LocalPath + @"\Melodyne\Transfer";
            string MelodyneFile = "melo";
            Directory.CreateDirectory(MelodyneFolder);
            File.CreateText(MelodyneFolder + @"\" + MelodyneFile).Close();

            await GitVersioning.uploadSongAsync(song, changeTitle, changeDescription, versionNumber);

            string shareLink = await GitVersioning.shareSongAsync(song);
            string expectedShareLink = "https://gitlab.com/MusicSyncTool/end-of-the-road.git";
            Assert.Equal(expectedShareLink,shareLink);

            string songFolder = @"SongDownloaded";

            string downloadDestination = testDirectory;
            await GitVersioning.downloadSharedSongAsync(songFolder, shareLink, downloadDestination);

            string expectedSongFile = downloadDestination + @"\" + songFolder + @"\" + songFile;
            Assert.True(File.Exists(expectedSongFile));
            string expectedMediaFile = downloadDestination + @"\" + songFolder + @"\Media\" + mediaFile;
            Assert.True(File.Exists(expectedMediaFile));
            string expectedMelodyneFile = downloadDestination + @"\" + songFolder + @"\Melodyne\Transfer\" + MelodyneFile;
            Assert.True(File.Exists(expectedMelodyneFile));
        }

        [Fact]
        public async Task uploadLockFileForASong()
        {

            string changeTitle = "Test";
            string changeDescription = "No Description";
            string versionNumber = "1.1.1";
            await GitVersioning.uploadSongAsync(song, changeTitle, changeDescription, versionNumber);

            changeTitle = "Lock";
            string lockFile = ".lock";
            File.CreateText(songLocalPath + @"\" + lockFile).Close();

            await GitVersioning.uploadSongAsync(song, lockFile, changeTitle);

            string shareLink = await GitVersioning.shareSongAsync(song);
            string expectedShareLink = "https://gitlab.com/MusicSyncTool/end-of-the-road.git";
            Assert.Equal(expectedShareLink, shareLink);

            string songFolder = @"SongDownloaded";

            string downloadDestination = testDirectory;
            await GitVersioning.downloadSharedSongAsync(songFolder, shareLink, downloadDestination);

            string expectedSongFile = downloadDestination + @"\" + songFolder + @"\" + lockFile;
            Assert.True(File.Exists(expectedSongFile));
        }

        [Fact]
        public async Task revertSong()
        {
            string changeTitle = "Test";
            string changeDescription = "No Description";
            string versionNumber = "1.1.1";
            await GitVersioning.uploadSongAsync(song, changeTitle, changeDescription, versionNumber);

            changeTitle = "Lock";
            string lockFile = ".lock";
            File.CreateText(songLocalPath + @"\" + lockFile).Close();

            await GitVersioning.uploadSongAsync(song, lockFile, changeTitle);

            File.Delete(song.LocalPath + @"\" + song.File);
            Assert.False(File.Exists(song.LocalPath + @"\" + song.File));

            await GitVersioning.revertSongAsync(song);

            Assert.True(File.Exists(song.LocalPath + @"\" + song.File));
        }

        [Fact]
        public async Task updateSong()
        {
            string changeTitle = "Test";
            string changeDescription = "No Description";
            string versionNumber = "1.1.1";
            await GitVersioning.uploadSongAsync(song, changeTitle, changeDescription, versionNumber);

            changeTitle = "Lock";
            string lockFile = ".lock";
            File.CreateText(songLocalPath + @"\" + lockFile).Close();

            await GitVersioning.uploadSongAsync(song, lockFile, changeTitle);

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("DELETE"), "https://gitlab.com/api/v4/projects/MusicSyncTool%2Fend-of-the-road/repository/files/.lock"))
                {
                    request.Headers.TryAddWithoutValidation("PRIVATE-TOKEN", "glpat-qwrrhK53iz4_mmSsx8h8");
                    request.Content = new StringContent("{\"branch\": \"master\", \"author_email\": \"author@example.com\", \"author_name\": \"Firstname Lastname\",\n       \"commit_message\": \"delete file\"}");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                    var response = await httpClient.SendAsync(request);
                }
            }

            System.Threading.Thread.Sleep(1500);

            Assert.True(File.Exists(songLocalPath + @"\" + lockFile));

            await GitVersioning.updateSongAsync(song);

            Assert.False(File.Exists(songLocalPath + @"\" + lockFile));
        }

        [Fact]
        public async Task initialVersionNumberTest()
        {
            List<(bool compo, bool mix, bool mastering, string expectedVersionNumber)> dataToBeTested = new List<(bool, bool, bool, string)>();
            dataToBeTested.Add((true, false, false, "1.0.0"));
            dataToBeTested.Add((false, true, false, "0.1.0"));
            dataToBeTested.Add((false, false, true, "0.0.1"));
            dataToBeTested.Add((true, true, true, "1.1.1"));

            foreach (var data in dataToBeTested)
            {
                string versionNumber = await GitVersioning.newVersionNumberAsync(song, data.compo, data.mix, data.mastering);
                Assert.Equal(data.expectedVersionNumber, versionNumber);
            }
        }

        [Fact]
        public async Task currentVersionTest()
        {
            string changeTitle = "Test";
            string changeDescription = "No Description";
            string versionNumber = "1.1.1";
            await GitVersioning.uploadSongAsync(song, changeTitle, changeDescription, versionNumber);

            SongVersion currentVersion = await GitVersioning.currentVersionAsync(song);

            Assert.Equal(versionNumber, currentVersion.Number);
            string expectedDescription = "Test\n\nNo Description";
            Assert.Equal(expectedDescription, currentVersion.Description);
            Assert.Equal(user.Username, currentVersion.Author);
        }

        [Fact]
        public async Task versionNumberTest()
        {
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
        }

        [Fact]
        public async Task versionsTest()
        {
            string changeTitle = "Test";
            string changeDescription = "No Description";
            string versionNumber = "1.1.1";
            await GitVersioning.uploadSongAsync(song, changeTitle, changeDescription, versionNumber);

            string changeTitle2 = "Test2";
            string changeDescription2 = "No Description2";
            string versionNumber2 = "1.2.1";
            string mediaFolder = song.LocalPath + @"\Media";
            string mediaFile = "guitar.wav";
            Directory.CreateDirectory(mediaFolder);
            File.CreateText(mediaFolder + @"\" + mediaFile).Close();
            await GitVersioning.uploadSongAsync(song, changeTitle2, changeDescription2, versionNumber2);

            List<SongVersion> versions = await GitVersioning.versionsAsync(song);
            
            List<SongVersion> expectedVersions = new List<SongVersion>();
            SongVersion songVersion1 = new SongVersion(versionNumber, changeTitle + "\n\n" + changeDescription, user.Username);
            SongVersion songVersion2 = new SongVersion(versionNumber2, changeTitle2 + "\n\n" + changeDescription2, user.Username);
            expectedVersions.Add(songVersion1);
            expectedVersions.Add(songVersion2);

            Assert.Equal(expectedVersions, versions);

        }
    }
}