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
using System.Net.Http.Headers;

namespace GitSongVersioningTests
{
    public abstract class TestsBase : IAsyncLifetime
    {
        protected TestsBase()
        {
            testDirectory = @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory\";
            songTitle = "End of the Road";
            songLocalPath = Path.Combine(testDirectory, songTitle) + Path.DirectorySeparatorChar;
            songFile = "file.song";           

            FileManager = new FileManager();
            song = new Song(songTitle, songFile, songLocalPath, Guid.NewGuid().ToString());
            songGuid = song.Guid.ToString();
            user = new User("MusicSyncTool", "HelloWorld12", "Clic", "musicsynctool@gmail.com");
            SaverMock = new Mock<ISaver>();
            SaverMock.Setup(m => m.SavedUser()).Returns(user);
            SaverMock.Setup(m => m.SavedMusicSyncFolder()).Returns(testDirectory);
            string gitServerUrl = "https://gitlab.com";
            ITransport gitTransport = new GitTransport(gitServerUrl, SaverMock.Object, FileManager);
            GitVersioning = new Versioning(SaverMock.Object, FileManager, gitTransport);
            FileManager.CreateDirectory(ref songLocalPath);
            FileManager.CreateFile(songFile, songLocalPath);
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await deleteGitlabProject();
            FileManager.DeleteDirectory(testDirectory);
        }

        public string testDirectory;
        public string songTitle;
        public string songLocalPath;
        public string songFile;
        public string songGuid;

        public User user;
        public Song song;
        public Mock<ISaver> SaverMock;
        public IFileManager FileManager;
        public Versioning GitVersioning;

        public async Task SimulateRemoteChangesAsync()
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("DELETE"), "https://gitlab.com/api/v4/projects/MusicSyncTool%2F" + song.Guid + "/repository/files/" + song.Guid + ".zip"))
                {
                    request.Headers.TryAddWithoutValidation("PRIVATE-TOKEN", "glpat-qwrrhK53iz4_mmSsx8h8");
                    request.Content = new StringContent("{\"branch\": \"master\", \"author_email\": \"author@example.com\", \"author_name\": \"Aymeric Meindre\",\n       \"commit_message\": \"delete file\\n\"}");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                    var response = await httpClient.SendAsync(request);
                }
            }

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://gitlab.com/api/v4/projects/MusicSyncTool%2F" + song.Guid + "/repository/tags?tag_name=2.0.0&ref=master"))
                {
                    request.Headers.TryAddWithoutValidation("PRIVATE-TOKEN", "glpat-qwrrhK53iz4_mmSsx8h8");

                    var response = await httpClient.SendAsync(request);
                }
            }
        }

        private async Task deleteGitlabProject()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            while(response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("DELETE"), "https://gitlab.com/api/v4/projects/MusicSyncTool%2F" + songGuid))
                    {
                        request.Headers.TryAddWithoutValidation("PRIVATE-TOKEN", "glpat-qwrrhK53iz4_mmSsx8h8");
                        response = await httpClient.SendAsync(request);
                    }
                }
            }
            System.Threading.Thread.Sleep(5000);
        }
    }

    [Collection("Serial")]
    public class GitSongVersioningTests : TestsBase
    {

        [Fact]
        public async Task uploadAndDownloadSong()
        {
            string changeTitle = "Test";
            string changeDescription = "No Description";
            string versionNumber = "1.1.1";

            string mediaFolder = song.LocalPath + @"Media\";
            string mediaFile = "guitar.wav";
            FileManager.CreateDirectory(ref mediaFolder);
            FileManager.CreateFile(mediaFile, mediaFolder);
            string MelodyneFolder = song.LocalPath + @"Melodyne\Transfer\";
            string MelodyneFile = "melo";
            FileManager.CreateDirectory(ref MelodyneFolder);
            FileManager.CreateFile(MelodyneFile,MelodyneFolder);

            await GitVersioning.UploadSongAsync(song, changeTitle, changeDescription, versionNumber);

            string shareLink = GitVersioning.ShareSong(song);
            string expectedShareLink = "https://gitlab.com/MusicSyncTool/" + song.Guid.ToString() + @".git";
            Assert.Equal(expectedShareLink,shareLink);

            FileManager.DeleteDirectory(song.LocalPath);
            string musicSyncSongFolder = testDirectory + ".musicsync/" + song.Guid + @"/";
            FileManager.DeleteDirectory(musicSyncSongFolder);

            string songPath = FileManager.FormatPath(testDirectory + song.Title);
            await GitVersioning.DownloadSharedSongAsync(shareLink, songPath);

            string expectedRepoPath = testDirectory + @".musicsync\" + song.Guid + @"\";
            Assert.True(Directory.Exists(expectedRepoPath));

            string expectedSongFile = songPath +  songFile;
            Assert.True(File.Exists(expectedSongFile));
            string expectedMediaFile = songPath + @"Media\" + mediaFile;
            Assert.True(File.Exists(expectedMediaFile));
            string expectedMelodyneFile = songPath + @"Melodyne\Transfer\" + MelodyneFile;
            Assert.True(File.Exists(expectedMelodyneFile));
        }

        [Fact]
        public async Task uploadLockFileForASong()
        {
            string changeTitle = "Test";
            string changeDescription = "No Description";
            string versionNumber = "1.1.1";
            await GitVersioning.UploadSongAsync(song, changeTitle, changeDescription, versionNumber);

            changeTitle = "Lock";
            string lockFile = ".lock";
            FileManager.CreateFile(lockFile,songLocalPath);

            await GitVersioning.UploadFileForSongAsync(song, lockFile, changeTitle);
            string shareLink = GitVersioning.ShareSong(song);

            string musicSyncFolder = testDirectory + ".musicsync/" + song.Guid.ToString();
            FileManager.DeleteDirectory(musicSyncFolder);
            string songPath = testDirectory + @"SongDownloaded\";
            await GitVersioning.DownloadSharedSongAsync(shareLink, songPath);

            string expectedLockFile = songPath +  lockFile;
            Assert.True(File.Exists(expectedLockFile));
        }

        [Fact]
        public async Task revertSong()
        {
            string changeTitle = "Test";
            string changeDescription = "No Description";
            string versionNumber = "1.1.1";
            await GitVersioning.UploadSongAsync(song, changeTitle, changeDescription, versionNumber);

            changeTitle = "Lock";
            string lockFile = ".lock";
            FileManager.CreateFile(lockFile, songLocalPath);

            await GitVersioning.UploadFileForSongAsync(song, lockFile, changeTitle);

            File.Delete(song.LocalPath + lockFile);
            File.Delete(song.LocalPath +  song.File);
            Assert.False(File.Exists(song.LocalPath + song.File));

            await GitVersioning.RevertSongAsync(song);

            Assert.True(File.Exists(song.LocalPath + song.File));
        }

        [Fact]
        public async Task updateSong()
        {
            string changeTitle = "Test";
            string changeDescription = "No Description";
            string versionNumber = "1.1.1";
            await GitVersioning.UploadSongAsync(song, changeTitle, changeDescription, versionNumber);

            changeTitle = "Lock";
            string lockFile = ".lock";
            FileManager.CreateFile(lockFile,songLocalPath);

            await GitVersioning.UploadFileForSongAsync(song, lockFile, changeTitle);

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("DELETE"), "https://gitlab.com/api/v4/projects/MusicSyncTool%2F" + song.Guid + "/repository/files/.lock"))
                {
                    request.Headers.TryAddWithoutValidation("PRIVATE-TOKEN", "glpat-qwrrhK53iz4_mmSsx8h8");
                    request.Content = new StringContent("{\"branch\": \"master\", \"author_email\": \"author@example.com\", \"author_name\": \"Firstname Lastname\",\n       \"commit_message\": \"delete file\"}");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                    var response = await httpClient.SendAsync(request);
                }
            }

            System.Threading.Thread.Sleep(1500);

            Assert.True(File.Exists(songLocalPath +  lockFile));

            await GitVersioning.UpdateSongAsync(song);

            Assert.False(File.Exists(songLocalPath +  lockFile));
        }

        [Fact]
        public async Task updateAvailableForSong()
        {
            string changeTitle = "Test";
            string changeDescription = "No Description";
            string versionNumber = "1.1.1";
            await GitVersioning.UploadSongAsync(song, changeTitle, changeDescription, versionNumber);

            changeTitle = "Lock";
            string lockFile = ".lock";
            FileManager.CreateFile(lockFile, songLocalPath);

            await GitVersioning.UploadFileForSongAsync(song, lockFile, changeTitle);

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("DELETE"), "https://gitlab.com/api/v4/projects/MusicSyncTool%2F" + song.Guid + "/repository/files/.lock"))
                {
                    request.Headers.TryAddWithoutValidation("PRIVATE-TOKEN", "glpat-qwrrhK53iz4_mmSsx8h8");
                    request.Content = new StringContent("{\"branch\": \"master\", \"author_email\": \"author@example.com\", \"author_name\": \"Firstname Lastname\",\n       \"commit_message\": \"delete file\"}");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                    var response = await httpClient.SendAsync(request);
                }
            }

            System.Threading.Thread.Sleep(1500);

            bool updatesAvailable = await GitVersioning.UpdatesAvailableForSongAsync(song);

            Assert.True(updatesAvailable);
        }

        [Fact]
        public async Task noUpdateAvailableForSong()
        {
            string changeTitle = "Test";
            string changeDescription = "No Description";
            string versionNumber = "1.1.1";
            await GitVersioning.UploadSongAsync(song, changeTitle, changeDescription, versionNumber);

            bool updatesAvailable = await GitVersioning.UpdatesAvailableForSongAsync(song);

            Assert.False(updatesAvailable);
        }

        [Fact]
        public async Task updateNoneDestructiveSong()
        {
            string changeTitle = "Test";
            string changeDescription = "No Description";
            string versionNumber = "1.1.1";
            await GitVersioning.UploadSongAsync(song, changeTitle, changeDescription, versionNumber);

            changeTitle = "Lock";
            string lockFile = ".lock";
            FileManager.CreateFile(lockFile, songLocalPath);

            await GitVersioning.UploadFileForSongAsync(song, lockFile, changeTitle);

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("DELETE"), "https://gitlab.com/api/v4/projects/MusicSyncTool%2F" + song.Guid + "/repository/files/.lock"))
                {
                    request.Headers.TryAddWithoutValidation("PRIVATE-TOKEN", "glpat-qwrrhK53iz4_mmSsx8h8");
                    request.Content = new StringContent("{\"branch\": \"master\", \"author_email\": \"author@example.com\", \"author_name\": \"Firstname Lastname\",\n       \"commit_message\": \"delete file\"}");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                    var response = await httpClient.SendAsync(request);
                }
            }

            System.Threading.Thread.Sleep(1500);

            Assert.True(File.Exists(songLocalPath +  lockFile));

            string randomFile = "randomFile.png";
            FileManager.CreateFile(randomFile, songLocalPath);
            string CacheFolder = "Cache";
            string folder = songLocalPath + CacheFolder;
            FileManager.CreateDirectory(ref folder);

            Assert.True(File.Exists(songLocalPath +  randomFile));
            Assert.True(Directory.Exists(songLocalPath +  CacheFolder));

            await GitVersioning.UpdateSongAsync(song);

            Assert.False(File.Exists(songLocalPath +  lockFile));
            Assert.True(File.Exists(songLocalPath +  randomFile));
            Assert.True(Directory.Exists(songLocalPath +  CacheFolder));
        }

        [Fact]
        public async Task currentVersionTest()
        {
            string changeTitle = "Test";
            string changeDescription = "No Description";
            string versionNumber = "1.1.1";
            await GitVersioning.UploadSongAsync(song, changeTitle, changeDescription, versionNumber);

            SongVersion currentVersion = await GitVersioning.CurrentVersionAsync(song);

            Assert.Equal(versionNumber, currentVersion.Number);
            string expectedDescription = "Test\n\nNo Description";
            Assert.Equal(expectedDescription, currentVersion.Description);
            Assert.Equal(user.Username, currentVersion.Author);
            DateOnly expectedDate = DateOnly.FromDateTime(DateTime.Today);
            Assert.Equal(expectedDate, currentVersion.Date);
        }

        [Fact]
        public async Task LocalCurrentVersionAfterRemoteUploadTest()
        {
            string changeTitle = "Test";
            string changeDescription = "No Description";
            string versionNumber = "1.1.1";
            await GitVersioning.UploadSongAsync(song, changeTitle, changeDescription, versionNumber);
            await SimulateRemoteChangesAsync();
            bool updatesAvailable = await GitVersioning.UpdatesAvailableForSongAsync(song);
            Assert.True(updatesAvailable);

            SongVersion currentVersion = await GitVersioning.CurrentVersionAsync(song);

            Assert.Equal(versionNumber, currentVersion.Number);
            string expectedDescription = "Test\n\nNo Description";
            Assert.Equal(expectedDescription, currentVersion.Description);
            Assert.Equal(user.Username, currentVersion.Author);
            DateOnly expectedDate = DateOnly.FromDateTime(DateTime.Today);
            Assert.Equal(expectedDate, currentVersion.Date);
        }

        [Fact]
        public async Task versionNumberTest()
        {
            string changeTitle = "Test";
            string changeDescription = "No Description";
            string versionNumber = "1.1.1";
            await GitVersioning.UploadSongAsync(song, changeTitle, changeDescription, versionNumber);

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
                versionNumber = await GitVersioning.NewVersionNumberAsync(song, data.compo, data.mix, data.mastering);
                Assert.Equal(data.expectedVersionNumber, versionNumber);
            }
        }

        [Fact]
        public async Task versionsTest()
        {
            string changeTitle = "Test";
            string changeDescription = "No Description";
            string versionNumber = "1.1.1";
            await GitVersioning.UploadSongAsync(song, changeTitle, changeDescription, versionNumber);

            string changeTitle2 = "Test2";
            string changeDescription2 = "No Description2";
            string versionNumber2 = "1.2.1";
            string mediaFolder = song.LocalPath + @"Media\";
            string mediaFile = "guitar.wav";
            FileManager.CreateDirectory(ref mediaFolder);
            FileManager.CreateFile(mediaFile, mediaFolder);
            await GitVersioning.UploadSongAsync(song, changeTitle2, changeDescription2, versionNumber2);

            List<SongVersion> versions = await GitVersioning.VersionsAsync(song);
            
            List<SongVersion> expectedVersions = new List<SongVersion>();

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            SongVersion songVersion1 = new SongVersion(versionNumber, changeTitle + "\n\n" + changeDescription, user.Username, today);
            SongVersion songVersion2 = new SongVersion(versionNumber2, changeTitle2 + "\n\n" + changeDescription2, user.Username, today);
            expectedVersions.Add(songVersion1);
            expectedVersions.Add(songVersion2);

            Assert.Equal(expectedVersions, versions);
        }

        [Fact]
        public async Task upcomingVersionsTest()
        {
            string changeTitle = "Test";
            string changeDescription = "No Description";
            string versionNumber = "1.1.1";
            await GitVersioning.UploadSongAsync(song, changeTitle, changeDescription, versionNumber);
            await SimulateRemoteChangesAsync();
            bool updatesAvailable = await GitVersioning.UpdatesAvailableForSongAsync(song);
            Assert.True(updatesAvailable);

            List<SongVersion> upcomingVersions = await GitVersioning.UpcomingVersionsAsync(song);

            SongVersion expectedUpcomingVersion = new SongVersion();
            expectedUpcomingVersion.Number = "2.0.0";
            expectedUpcomingVersion.Author = "Aymeric Meindre";
            expectedUpcomingVersion.Description = "delete file";
            DateOnly expectedDate = DateOnly.FromDateTime(DateTime.Today);
            expectedUpcomingVersion.Date = expectedDate;

            Assert.Contains(expectedUpcomingVersion, upcomingVersions);
        }

        [Fact]
        public async Task noUpcomingVersionsTest()
        {
            string changeTitle = "Test";
            string changeDescription = "No Description";
            string versionNumber = "1.1.1";
            await GitVersioning.UploadSongAsync(song, changeTitle, changeDescription, versionNumber);

            bool updatesAvailable = await GitVersioning.UpdatesAvailableForSongAsync(song);
            Assert.False(updatesAvailable);

            List<SongVersion> upcomingVersions = await GitVersioning.UpcomingVersionsAsync(song);

            Assert.Empty(upcomingVersions);
        }
    }
}