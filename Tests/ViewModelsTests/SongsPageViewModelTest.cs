using App1.Models;
using App1.Models.Ports;
using App1.ViewModels;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ViewModelsTests.SongsPageViewModelTest
{
    public class SongsPageViewModelTest
    {

        [Fact]
        public void InitializedSongsVersionedWithExistingSongsTest()
        {
            //Setup
            string title = "title";
            string file = "file";
            string localPath = "localPath";
            var song = new Song(title, file, localPath);
            string title2 = "title2";
            string file2 = "file2";
            string localPath2 = "localPath2";
            var song2 = new Song(title2, file2, localPath2);
            var saverMock = new Mock<ISaver>();
            var songsList = new List<Song>
            {
                song,
                song2
            };
            saverMock.Setup(m => m.SavedSongs()).Returns(songsList);
            var songs = new SongsStorage(saverMock.Object);
            var songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.SongList).Returns(songs);

            var viewModel = new SongsPageViewModel(songsManagerMock.Object);

            var expectedSong = new SongVersioned(title);
            var expectedSong2 = new SongVersioned(title2);

            Assert.Contains(expectedSong, viewModel.SongsVersioned);
            Assert.Contains(expectedSong2, viewModel.SongsVersioned);
        }

        [Fact]
        public async Task refreshSongsVersionedWithExistingSongsTest()
        {
            //Setup
            string title = "title";
            string file = "file";
            string localPath = "localPath";
            var songVersion = new SongVersion
            {
                Number = "1.0.0",
                Author = "Oregano",
                Description = "No description",
                Date = new DateOnly(2022, 5, 10)
            };
            var songsVersion = new List<SongVersion>
            {
                songVersion
            };
            var song = new Song(title, file, localPath);
            song.Status.state = SongStatus.State.locked;
            song.Status.whoLocked = "Oregano";
            string title2 = "title2";
            string file2 = "file2";
            string localPath2 = "localPath2";
            var songVersion2 = new SongVersion
            {
                Number = "2.1.0",
                Author = "Aymeric Meindre",
                Description = "Test new mix",
                Date = new DateOnly(2022, 5, 11)
            };
            var songsVersion2 = new List<SongVersion>
            {
                songVersion,
                songVersion2
            };
            var upcomingSongVersion2 = new SongVersion
            {
                Number = "3.1.0",
                Author = "Oregano",
                Description = "New Composition",
                Date = new DateOnly(2022, 5, 12)
            };
            ;
            var upcomingSongsVersion = new List<SongVersion>();
            var upcomingSongsVersion2 = new List<SongVersion>
            {
                upcomingSongVersion2
            };
            var song2 = new Song(title2, file2, localPath2);
            song2.Status.state = SongStatus.State.updatesAvailable;
            var saverMock = new Mock<ISaver>();
            var songsList = new List<Song>
            {
                song,
                song2
            };
            saverMock.Setup(m => m.SavedSongs()).Returns(songsList);
            var songs = new SongsStorage(saverMock.Object);
            var songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.SongList).Returns(songs);
            songsManagerMock.Setup(m => m.FindSong(song.Title)).Returns(song);
            songsManagerMock.Setup(m => m.CurrentVersionAsync(song)).Returns(Task.FromResult(songVersion));
            songsManagerMock.Setup(m => m.VersionsAsync(song)).Returns(Task.FromResult(songsVersion));
            songsManagerMock.Setup(m => m.FindSong(song2.Title)).Returns(song2);
            songsManagerMock.Setup(m => m.CurrentVersionAsync(song2)).Returns(Task.FromResult(songVersion2));
            songsManagerMock.Setup(m => m.VersionsAsync(song2)).Returns(Task.FromResult(songsVersion2));
            songsManagerMock.Setup(m => m.UpcomingVersionsAsync(song)).Returns(Task.FromResult(upcomingSongsVersion));
            songsManagerMock.Setup(m => m.UpcomingVersionsAsync(song2)).Returns(Task.FromResult(upcomingSongsVersion2));

            var viewModel = new SongsPageViewModel(songsManagerMock.Object);
            await viewModel.RefreshSongsVersionedAsync();

            var expectedSong = new SongVersioned(title);
            var expectedVersion = new App1.ViewModels.Version();
            expectedVersion.Number = "1.0.0";
            Assert.Equal("Compo v1", expectedVersion.Number);
            expectedVersion.Author = "Oregano";
            expectedVersion.Description = "No description";
            expectedVersion.Date = "5/10/2022";
            expectedSong.Status = "Locked by Oregano";
            SongVersioned expectedSong2 = new SongVersioned(title2);
            App1.ViewModels.Version expectedVersion2 = new App1.ViewModels.Version();
            expectedVersion2.Number = "2.1.0";
            Assert.Equal("Compo v2 / Mix v1", expectedVersion2.Number);
            expectedVersion2.Author = "Aymeric Meindre";
            expectedVersion2.Description = "Test new mix";
            expectedVersion2.Date = "5/11/2022";
            expectedSong2.Status = "Updates Available";
            List<App1.ViewModels.Version> expectedVersions = new List<App1.ViewModels.Version>();
            expectedVersions.Add(expectedVersion);
            List<App1.ViewModels.Version> expectedVersions2 = new List<App1.ViewModels.Version>();
            expectedVersions2.Add(expectedVersion2);
            expectedVersions2.Add(expectedVersion);
            App1.ViewModels.Version expectedUpcomingVersion2 = new App1.ViewModels.Version();
            expectedUpcomingVersion2.Number = "3.1.0";
            Assert.Equal("Compo v3 / Mix v1", expectedUpcomingVersion2.Number);
            expectedUpcomingVersion2.Author = "Oregano";
            expectedUpcomingVersion2.Description = "New Composition";
            expectedUpcomingVersion2.Date = "5/12/2022";
            List<App1.ViewModels.Version> expectedUpcomingVersions2 = new List<App1.ViewModels.Version>();
            expectedUpcomingVersions2.Add(expectedUpcomingVersion2);

            SongVersioned songVersioned = viewModel.SongsVersioned.First(m => m.Equals(expectedSong));
            SongVersioned songVersioned2 = viewModel.SongsVersioned.First(m => m.Equals(expectedSong2));
            Assert.Equal(expectedVersion, songVersioned.CurrentVersion);
            Assert.Equal(expectedVersion2, songVersioned2.CurrentVersion);
            Assert.Equal(expectedVersions, songVersioned.Versions);
            Assert.Equal(expectedVersions2, songVersioned2.Versions);
            Assert.Equal(expectedUpcomingVersions2, songVersioned2.UpcomingVersions);
            Assert.Equal(expectedSong.Status, songVersioned.Status);
            Assert.Equal(expectedSong2.Status, songVersioned2.Status);
        }

        [Fact]
        public async Task refreshSongErrorTest()
        {
            //Setup
            string title = "title";
            string file = "file";
            string localPath = "localPath";
            SongVersion songVersion = new SongVersion();
            songVersion.Number = "1.0.0";
            songVersion.Author = "Oregano";
            songVersion.Description = "No description";
            List<SongVersion> songsVersion = new List<SongVersion>();
            songsVersion.Add(songVersion);
            Song song = new Song(title, file, localPath);
            song.Status.state = SongStatus.State.locked;
            song.Status.whoLocked = "Oregano";
            Mock<ISaver> saverMock = new Mock<ISaver>();
            List<Song> songsList = new List<Song>();
            songsList.Add(song);
            saverMock.Setup(m => m.SavedSongs()).Returns(songsList);
            SongsStorage songs = new SongsStorage(saverMock.Object);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.SongList).Returns(songs);
            songsManagerMock.Setup(m => m.FindSong(song.Title)).Returns(song);
            songsManagerMock.Setup(m => m.CurrentVersionAsync(song)).Throws(new Exception());
            songsManagerMock.Setup(m => m.VersionsAsync(song)).Returns(Task.FromResult(songsVersion));
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);

            //Add a new song
            await Assert.ThrowsAnyAsync<Exception>(async () => await viewModel.RefreshSongsVersionedAsync());

            SongVersioned expectedSong = new SongVersioned(title);
            Assert.Equal("Error", viewModel.SongsVersioned.First(m => m.Equals(expectedSong)).Status);
            Assert.False(viewModel.SongsVersioned.First(m => m.Equals(expectedSong)).IsRefreshingSong);

        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road")]
        [InlineData("End of the Road", "test.song", @"User/test/End of the Road")]
        public async void addLocalSongAsyncTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.FindSong(title)).Returns(song);
            SongVersion songVersion = new SongVersion("1.0.0", string.Empty, "Aymeric", new DateOnly(2022, 5, 11));
            songsManagerMock.Setup(m => m.CurrentVersionAsync(song)).Returns(Task.FromResult(songVersion));
            List<SongVersion> upcomingSongVersions = new List<SongVersion>();
            songsManagerMock.Setup(m => m.UpcomingVersionsAsync(song)).Returns(Task.FromResult(upcomingSongVersions));
            List<SongVersion> songVersions = new List<SongVersion>();
            songVersions.Add(songVersion);
            songsManagerMock.Setup(m => m.VersionsAsync(song)).Returns(Task.FromResult(songVersions));

            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);

            //Add a new song
            SongVersioned songVersioned = await viewModel.AddLocalSongAsync(title, file, localPath);

            //We expect a songVersioned created with the title
            SongVersioned expectedSongVersioned = new SongVersioned(title);
            Assert.Equal(expectedSongVersioned, songVersioned);
            Assert.Equal("Compo v1", songVersioned.CurrentVersion.Number);
            Assert.Equal(String.Empty, songVersioned.CurrentVersion.Description);
            Assert.Contains(expectedSongVersioned, viewModel.SongsVersioned);
            Assert.False(viewModel.IsAddingSong);
            //We expect to have called the await addLocalSongAsync method in the songsManager
            songsManagerMock.Verify(m => m.AddLocalSongAsync(title, file, localPath + '\\'), Times.Once());
            Action action = async () => await viewModel.AddLocalSongAsync(title, file, localPath);
            Assert.PropertyChanged(viewModel, "IsAddingSong", action);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road")]
        [InlineData("End of the Road", "test.song", @"User/test/End of the Road")]
        public void addLocalSongAsyncErrorTest(string title, string file, string localPath)
        {
            //Setup
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.AddLocalSongAsync(title, file, localPath + '\\')).Throws(new Exception());
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);

            //Add a new song
            Assert.ThrowsAsync<Exception>(() => viewModel.AddLocalSongAsync(title, file, localPath));

            //We expect a songVersioned created with the title
            SongVersioned expectedSongVersioned = new SongVersioned(title);
            Assert.DoesNotContain(expectedSongVersioned, viewModel.SongsVersioned);
            Assert.False(viewModel.IsAddingSong);
            //We expect to have called the await addLocalSongAsync method in the songsManager
            songsManagerMock.Verify(m => m.AddLocalSongAsync(title, file, localPath + '\\'), Times.Once());
        }

        [Theory]
        [InlineData("title", "http://www.yoursong.com", @"./SongsManagerTest")]
        [InlineData("End of the Road", "http://www.end-of-the-road.com", @"./SongsManagerTest")]
        public async Task addSharedSongTest(string title, string link, string downloadPath)
        {
            //Setup
            Song song = new Song(title, "file.Song", downloadPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.FindSong(title)).Returns(song);
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            string expectedDescription = "New Version from an User";
            string expectedNumber = "v2.3.0";
            string expectedAuthor = "Oregano";
            DateOnly expectedDate = new DateOnly(2022, 5, 11);
            SongVersion songVersion = new SongVersion(expectedNumber, expectedDescription, expectedAuthor, expectedDate);
            songsManagerMock.Setup(m => m.CurrentVersionAsync(song)).Returns(Task.FromResult(songVersion));
            List<SongVersion> songVersions = new List<SongVersion>();
            songsManagerMock.Setup(m => m.VersionsAsync(song)).Returns(Task.FromResult(songVersions));
            List<SongVersion> upcomingSongVersions = new List<SongVersion>();
            songsManagerMock.Setup(m => m.UpcomingVersionsAsync(song)).Returns(Task.FromResult(upcomingSongVersions));

            //Add a new song
            await viewModel.AddSharedSongAsync(title, link, downloadPath);

            //We expect a songVersioned created with the title
            SongVersioned expectedSongVersioned = new SongVersioned(title);
            Assert.Contains(expectedSongVersioned, viewModel.SongsVersioned);
            //We expect to have called the addSharedSongAsync method in the songsManager
            songsManagerMock.Verify(m => m.AddSharedSongAsync(title, link, downloadPath + '\\'), Times.Once());
            songsManagerMock.Verify(m => m.UpdateSongAsync(song), Times.Once());
            songsManagerMock.Verify(m => m.CurrentVersionAsync(song), Times.Once());
            Action action = async () => await viewModel.AddSharedSongAsync(title, link, downloadPath);
            Assert.PropertyChanged(viewModel, "IsAddingSong", action);
        }

        [Theory]
        [InlineData("title", "ERROR", @"./SongsManagerTest")]
        public async Task addSharedSongErrorTest(string title, string link, string downloadPath)
        {
            //Setup
            Song song = new Song(title, "file.Song", downloadPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.AddSharedSongAsync(title, link, downloadPath + '\\')).Throws(new Exception());
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);

            //Add a new song
            await Assert.ThrowsAnyAsync<Exception>(async () => await viewModel.AddSharedSongAsync(title, link, downloadPath));

            //We expect a songVersioned created with the title
            SongVersioned expectedSongVersioned = new SongVersioned(title);
            Assert.DoesNotContain(expectedSongVersioned, viewModel.SongsVersioned);
            //We expect to have called the addSharedSongAsync method in the songsManager
            songsManagerMock.Verify(m => m.AddSharedSongAsync(title, link, downloadPath + '\\'), Times.Once());
            songsManagerMock.Verify(m => m.UpdateSongAsync(song), Times.Never());
            songsManagerMock.Verify(m => m.CurrentVersionAsync(song), Times.Never());
            Action action = async () => await viewModel.AddSharedSongAsync(title, link, downloadPath);
            Assert.PropertyChanged(viewModel, "IsAddingSong", action);
            Assert.False(viewModel.IsAddingSong);
        }

        [Fact]
        public void shareLinkSongTest()
        {
            //Setup
            Song song = new Song("title", "file.Song", "LocalPath");
            SongVersioned songVersioned = new SongVersioned("title");
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.FindSong("title")).Returns(song);
            string expectedShareLink = @"https://www.gitlab.com/test.git";
            songsManagerMock.Setup(m => m.ShareSong(song)).Returns(expectedShareLink);
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);

            //Add a new song
            string shareLink = viewModel.ShareSong(songVersioned);

            Assert.Equal(expectedShareLink, shareLink);
            //We expect to have called the addSharedSongAsync method in the songsManager
            songsManagerMock.Verify(m => m.ShareSong(song), Times.Once());
        }

        [Fact]
        public void shareLinkSongErrorTest()
        {
            //Setup
            var title = "title";
            Song song = new Song(title, "file.Song", "LocalPath");
            SongVersioned songVersioned = new SongVersioned(title);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.FindSong(title)).Returns(song);
            songsManagerMock.Setup(m => m.ShareSong(song)).Throws(new Exception());
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);

            //Add a new song
            Assert.ThrowsAny<Exception>(() => viewModel.ShareSong(songVersioned));

            Assert.Equal("Error", songVersioned.Status);
            //We expect to have called the addSharedSongAsync method in the songsManager
            songsManagerMock.Verify(m => m.ShareSong(song), Times.Once());
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task updateSongAsyncTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.FindSong(title)).Returns(song);
            string expectedDescription = "New Version from an User";
            string expectedNumber = "2.3.0";
            string expectedVersionNumber = "Compo v2 / Mix v3";
            string expectedAuthor = "Oregano";
            string expectedDate = "5/11/2022";
            DateOnly date = new DateOnly(2022, 5, 11);
            SongVersion songVersion = new SongVersion(expectedNumber, expectedDescription, expectedAuthor, date);
            songsManagerMock.Setup(m => m.CurrentVersionAsync(song)).Returns(Task.FromResult(songVersion));
            List<SongVersion> songVersions = new List<SongVersion>();
            songsManagerMock.Setup(m => m.VersionsAsync(song)).Returns(Task.FromResult(songVersions));
            List<SongVersion> upcomingSongVersions = new List<SongVersion>();
            songsManagerMock.Setup(m => m.UpcomingVersionsAsync(song)).Returns(Task.FromResult(upcomingSongVersions));
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            await viewModel.AddLocalSongAsync(title, file, localPath);
            SongVersioned expectedSongToBeUpdated = new SongVersioned(title);

            await viewModel.UpdateSongAsync(expectedSongToBeUpdated);

            Assert.Equal(string.Empty, expectedSongToBeUpdated.Status);
            Assert.Equal(expectedDescription, expectedSongToBeUpdated.CurrentVersion.Description);
            Assert.Equal(expectedVersionNumber, expectedSongToBeUpdated.CurrentVersion.Number);
            Assert.Equal(expectedAuthor, expectedSongToBeUpdated.CurrentVersion.Author);
            Assert.Equal(expectedDate, expectedSongToBeUpdated.CurrentVersion.Date);
            songsManagerMock.Verify(m => m.UpdateSongAsync(song), Times.Once());
            songsManagerMock.Verify(m => m.CurrentVersionAsync(song), Times.Exactly(2));
            Action action = async () => await viewModel.UpdateSongAsync(expectedSongToBeUpdated);
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
            songsManagerMock.Setup(m => m.FindSong(title)).Returns(song);
            SongVersion version = new SongVersion("1.0.0", "No Description", "Aymeric", new DateOnly(2022, 5, 11));
            songsManagerMock.Setup(m => m.CurrentVersionAsync(song)).Returns(Task.FromResult(version));
            List<SongVersion> songVersions = new List<SongVersion>();
            songsManagerMock.Setup(m => m.VersionsAsync(song)).Returns(Task.FromResult(songVersions));
            List<SongVersion> upcomingSongVersions = new List<SongVersion>();
            songsManagerMock.Setup(m => m.UpcomingVersionsAsync(song)).Returns(Task.FromResult(upcomingSongVersions));
            SongVersioned expectedSongToBeUpdated = new SongVersioned(title);
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            await viewModel.AddLocalSongAsync(title, file, localPath);


            await viewModel.UpdateSongAsync(expectedSongToBeUpdated);

            Assert.Equal("Locked by Oregano", expectedSongToBeUpdated.Status);
            songsManagerMock.Verify(m => m.UpdateSongAsync(song), Times.Once());
            Action action = async () => await viewModel.UpdateSongAsync(expectedSongToBeUpdated);
            Assert.PropertyChanged(expectedSongToBeUpdated, "Status", action);
            Assert.PropertyChanged(expectedSongToBeUpdated, "IsLoading", action);
        }

        [Theory]
        [InlineData("LockedSong", "file.song", @"./SongsManagerTest/End of the Road/")]
        public async Task updateSongAsyncErrorTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.FindSong(title)).Returns(song);
            songsManagerMock.Setup(m => m.UpdateSongAsync(song)).Throws(new Exception());
            songsManagerMock.Setup(m => m.FindSong(title)).Returns(song);
            SongVersion version = new SongVersion("1.0.0", "No Description", "Aymeric", new DateOnly(2022, 5, 11));
            songsManagerMock.Setup(m => m.CurrentVersionAsync(song)).Returns(Task.FromResult(version));
            List<SongVersion> songVersions = new List<SongVersion>();
            songsManagerMock.Setup(m => m.VersionsAsync(song)).Returns(Task.FromResult(songVersions));
            List<SongVersion> upcomingSongVersions = new List<SongVersion>();
            songsManagerMock.Setup(m => m.UpcomingVersionsAsync(song)).Returns(Task.FromResult(upcomingSongVersions));

            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            await viewModel.AddLocalSongAsync(title, file, localPath);
            SongVersioned expectedSongToBeUpdated = new SongVersioned(title);

            await Assert.ThrowsAnyAsync<Exception>(async () => await viewModel.UpdateSongAsync(expectedSongToBeUpdated));

            Assert.Equal("Error", expectedSongToBeUpdated.Status);
            Assert.False(expectedSongToBeUpdated.IsUpdatingSong);
            songsManagerMock.Verify(m => m.UpdateSongAsync(song), Times.Once());
            Action action = async () => await viewModel.UpdateSongAsync(expectedSongToBeUpdated);
            Assert.PropertyChanged(expectedSongToBeUpdated, "Status", action);
            Assert.PropertyChanged(expectedSongToBeUpdated, "IsLoading", action);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task deleteSongAsyncTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.FindSong(title)).Returns(song);
            SongVersion songVersion = new SongVersion("1.0.0", string.Empty, "Aymeric", new DateOnly(2022, 5, 11));
            songsManagerMock.Setup(m => m.CurrentVersionAsync(song)).Returns(Task.FromResult(songVersion));
            List<SongVersion> songVersions = new List<SongVersion>();
            songVersions.Add(songVersion);
            songsManagerMock.Setup(m => m.VersionsAsync(song)).Returns(Task.FromResult(songVersions));
            List<SongVersion> upcomingSongVersions = new List<SongVersion>();
            songsManagerMock.Setup(m => m.UpcomingVersionsAsync(song)).Returns(Task.FromResult(upcomingSongVersions));

            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            await viewModel.AddLocalSongAsync(title, file, localPath);
            SongVersioned expectedSongToBeDeleted = new SongVersioned(title);

            await viewModel.DeleteSongAsync(expectedSongToBeDeleted);

            Assert.DoesNotContain(expectedSongToBeDeleted, viewModel.SongsVersioned);
            songsManagerMock.Verify(m => m.DeleteSongAsync(song), Times.Once());
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task deleteSongAsyncErrorTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.FindSong(title)).Throws(new Exception());
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            SongVersioned expectedSongToBeDeleted = new SongVersioned(title);

            await Assert.ThrowsAnyAsync<Exception>(async () => await viewModel.DeleteSongAsync(expectedSongToBeDeleted));

            Assert.Equal("Error", expectedSongToBeDeleted.Status);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task openSongAsyncTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.FindSong(title)).Returns(song);
            songsManagerMock.Setup(m => m.OpenSongAsync(song)).Returns(Task.FromResult(true));
            SongVersion songVersion = new SongVersion("1.0.0", string.Empty, "Aymeric", new DateOnly(2022, 5, 11));
            songsManagerMock.Setup(m => m.CurrentVersionAsync(song)).Returns(Task.FromResult(songVersion));
            List<SongVersion> songVersions = new List<SongVersion>();
            songVersions.Add(songVersion);
            songsManagerMock.Setup(m => m.VersionsAsync(song)).Returns(Task.FromResult(songVersions));
            List<SongVersion> upcomingSongVersions = new List<SongVersion>();
            songsManagerMock.Setup(m => m.UpcomingVersionsAsync(song)).Returns(Task.FromResult(upcomingSongVersions));

            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            await viewModel.AddLocalSongAsync(title, file, localPath);
            SongVersioned expectedSongToBeOpened = new SongVersioned(title);

            await viewModel.OpenSongAsync(expectedSongToBeOpened);

            songsManagerMock.Verify(m => m.OpenSongAsync(song), Times.Once());
            Action action = async () => await viewModel.OpenSongAsync(expectedSongToBeOpened);
            Assert.PropertyChanged(expectedSongToBeOpened, "Status", action);
            Assert.PropertyChanged(expectedSongToBeOpened, "IsLoading", action);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task openSongAsyncErrorTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.FindSong(title)).Returns(song);
            songsManagerMock.Setup(m => m.OpenSongAsync(song)).Throws(new Exception());
            SongVersion songVersion = new SongVersion("1.0.0", string.Empty, "Aymeric", new DateOnly(2022, 5, 11));
            songsManagerMock.Setup(m => m.CurrentVersionAsync(song)).Returns(Task.FromResult(songVersion));
            List<SongVersion> songVersions = new List<SongVersion>();
            songVersions.Add(songVersion);
            songsManagerMock.Setup(m => m.VersionsAsync(song)).Returns(Task.FromResult(songVersions));
            List<SongVersion> upcomingSongVersions = new List<SongVersion>();
            songsManagerMock.Setup(m => m.UpcomingVersionsAsync(song)).Returns(Task.FromResult(upcomingSongVersions));

            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            await viewModel.AddLocalSongAsync(title, file, localPath);
            SongVersioned expectedSongToBeOpened = new SongVersioned(title);

            await Assert.ThrowsAnyAsync<Exception>(async () => await viewModel.OpenSongAsync(expectedSongToBeOpened));

            songsManagerMock.Verify(m => m.OpenSongAsync(song), Times.Once());
            Assert.Equal("Error", expectedSongToBeOpened.Status);
            Assert.False(expectedSongToBeOpened.IsOpeningSong);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task revertSongAsyncTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.FindSong(title)).Returns(song);
            SongVersion songVersion = new SongVersion("1.0.0", string.Empty, "Aymeric", new DateOnly(2022, 5, 11));
            songsManagerMock.Setup(m => m.CurrentVersionAsync(song)).Returns(Task.FromResult(songVersion));
            List<SongVersion> songVersions = new List<SongVersion>();
            songVersions.Add(songVersion);
            songsManagerMock.Setup(m => m.VersionsAsync(song)).Returns(Task.FromResult(songVersions));
            List<SongVersion> upcomingSongVersions = new List<SongVersion>();
            songsManagerMock.Setup(m => m.UpcomingVersionsAsync(song)).Returns(Task.FromResult(upcomingSongVersions));

            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            await viewModel.AddLocalSongAsync(title, file, localPath);
            SongVersioned expectedSongToBeReverted = new SongVersioned(title);

            await viewModel.RevertSongAsync(expectedSongToBeReverted);

            songsManagerMock.Verify(m => m.RevertSongAsync(song), Times.Once());
            Action action = async () => await viewModel.RevertSongAsync(expectedSongToBeReverted);
            Assert.PropertyChanged(expectedSongToBeReverted, "Status", action);
            Assert.PropertyChanged(expectedSongToBeReverted, "IsLoading", action);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task revertSongAsyncErrorTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.FindSong(title)).Returns(song);
            songsManagerMock.Setup(m => m.RevertSongAsync(song)).Throws(new Exception());
            SongVersion songVersion = new SongVersion("1.0.0", string.Empty, "Aymeric", new DateOnly(2022, 5, 11));
            songsManagerMock.Setup(m => m.CurrentVersionAsync(song)).Returns(Task.FromResult(songVersion));
            List<SongVersion> songVersions = new List<SongVersion>();
            songVersions.Add(songVersion);
            songsManagerMock.Setup(m => m.VersionsAsync(song)).Returns(Task.FromResult(songVersions));
            List<SongVersion> upcomingSongVersions = new List<SongVersion>();
            songsManagerMock.Setup(m => m.UpcomingVersionsAsync(song)).Returns(Task.FromResult(upcomingSongVersions));

            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            await viewModel.AddLocalSongAsync(title, file, localPath);
            SongVersioned expectedSongToBeReverted = new SongVersioned(title);

            await Assert.ThrowsAnyAsync<Exception>(async () => await viewModel.RevertSongAsync(expectedSongToBeReverted));

            songsManagerMock.Verify(m => m.RevertSongAsync(song), Times.Once());
            Assert.Equal("Error", expectedSongToBeReverted.Status);
            Assert.False(expectedSongToBeReverted.IsRevertingSong);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task uploadNewSongVersionAsyncTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.FindSong(title)).Returns(song);
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            List<SongVersion> songVersions = new List<SongVersion>();
            SongVersion version = new SongVersion("1.0.0", "No Description", "Oregano", new DateOnly(2022, 5, 11));
            songVersions.Add(version);
            songsManagerMock.Setup(m => m.VersionsAsync(song)).Returns(Task.FromResult(songVersions));
            songsManagerMock.Setup(m => m.CurrentVersionAsync(song)).Returns(Task.FromResult(version));
            List<SongVersion> upcomingSongVersions = new List<SongVersion>();
            songsManagerMock.Setup(m => m.UpcomingVersionsAsync(song)).Returns(Task.FromResult(upcomingSongVersions));

            SongVersioned expectedSongToBeUploaded = new SongVersioned(title);
            string changeTitle = "New Title";
            string changeDescritpion = "No Description";

            await viewModel.UploadNewSongVersionAsync(expectedSongToBeUploaded, changeTitle, changeDescritpion, true, false, false);

            songsManagerMock.Verify(m => m.UploadNewSongVersionAsync(song, changeTitle, changeDescritpion, true, false, false), Times.Once());
            Action action = async () => await viewModel.UploadNewSongVersionAsync(expectedSongToBeUploaded, changeTitle, changeDescritpion, true, false, false);
            Assert.PropertyChanged(expectedSongToBeUploaded, "Status", action);
            Assert.PropertyChanged(expectedSongToBeUploaded, "IsLoading", action);
        }

        [Theory]
        [InlineData("title", "file.song", @"./SongsManagerTest/End of the Road/")]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task uploadNewSongVersionAsyncErrorTest(string title, string file, string localPath)
        {
            //Setup
            Song song = new Song(title, file, localPath);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.FindSong(title)).Returns(song);
            SongVersion songVersion = new SongVersion("1.0.0", string.Empty, "Aymeric", new DateOnly(2022, 5, 11));
            songsManagerMock.Setup(m => m.CurrentVersionAsync(song)).Returns(Task.FromResult(songVersion));
            List<SongVersion> songVersions = new List<SongVersion>();
            songVersions.Add(songVersion);
            songsManagerMock.Setup(m => m.VersionsAsync(song)).Returns(Task.FromResult(songVersions));
            string changeTitle = "New Title";
            string changeDescritpion = "No Description";
            songsManagerMock.Setup(m => m.UploadNewSongVersionAsync(song, changeTitle, changeDescritpion, true, false, false)).Throws(new Exception());
            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);
            SongVersioned expectedSongToBeUploaded = new SongVersioned(title);

            await Assert.ThrowsAnyAsync<Exception>(async () => await viewModel.UploadNewSongVersionAsync(expectedSongToBeUploaded, changeTitle, changeDescritpion, true, false, false));

            songsManagerMock.Verify(m => m.UploadNewSongVersionAsync(song, changeTitle, changeDescritpion, true, false, false), Times.Once());
            Assert.Equal("Error", expectedSongToBeUploaded.Status);
            Assert.False(expectedSongToBeUploaded.IsUploadingSong);
        }

        [Fact]
        public void RenameSongTest()
        {
            //Setup
            string title = "title";
            string file = "file";
            string localPath = "localPath";
            Song song = new Song(title, file, localPath);
            Mock<ISaver> saverMock = new Mock<ISaver>();
            List<Song> songsList = new List<Song>();
            songsList.Add(song);

            string newTitle = "new Title";
            Song newSong = new Song(newTitle, file, localPath);
            List<Song> songsList2 = new List<Song>();
            songsList.Add(newSong);
            saverMock.SetupSequence(m => m.SavedSongs()).Returns(songsList).Returns(songsList2);
            SongsStorage songs = new SongsStorage(saverMock.Object);
            Mock<ISongsManager> songsManagerMock = new Mock<ISongsManager>();
            songsManagerMock.Setup(m => m.SongList).Returns(songs);

            SongsPageViewModel viewModel = new SongsPageViewModel(songsManagerMock.Object);

            SongVersioned songVersioned = new SongVersioned(title);

            viewModel.RenameSong(songVersioned, newTitle);

            Assert.Equal(newTitle, songVersioned.Title);
            Action action = () => viewModel.RenameSong(songVersioned, newTitle);
            Assert.PropertyChanged(songVersioned, "Title", action);

            SongVersioned newNamedSongVersioned = new SongVersioned(newTitle);
            Assert.Contains(newNamedSongVersioned, viewModel.SongsVersioned);
        }
    }
}