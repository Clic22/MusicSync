﻿using App1.Models;
using App1.Models.Ports;
using App1Tests.Mock;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace ModelsTests.SongsManagerTest
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
            user = new User(BandName, BandPassword, Username, BandEmail);

            title = "End of the Road";
            file = "file.song";
            localPath = @"./SongsManagerTest/End of the Road/";
            Directory.CreateDirectory(localPath);
            FileStream fileStream = File.Create(localPath + file);
            fileStream.Close();
            expectedSong = new Song(title, file, localPath);

            version = new VersioningMock(user);
            saver = new SaverMock();
            string musicFolder = "TestFolder";
            saver.saveSettings(user, musicFolder);
            fileManager = new Mock<IFileManager>();
            songsManager = new SongsManager(version, saver, fileManager.Object);
            locker = new Locker(version);
        }

        public void Dispose()
        {
            if (expectedSong.LocalPath != null)
            {
                Directory.Delete(expectedSong.LocalPath, true);
            }
            if (Directory.Exists(version.versionPath + expectedSong.LocalPath))
            {
                Directory.Delete(version.versionPath + expectedSong.LocalPath, true);
            }
            songsManager.SongList.Clear();
        }

        public User user;
        public Song expectedSong;
        public ISaver saver;
        public Mock<IFileManager> fileManager;
        public SongsManager songsManager;
        public VersioningMock version;
        public Locker locker;
        public string title;
        public string file;
        public string localPath;
    }

    public class SongsManagerTest : TestsBase
    {
        [Fact]
        public void addAndFindSongTest()
        {
            songsManager.addLocalSong(title, file, localPath);
            Song song = songsManager.findSong(title);
            Assert.Equal(expectedSong, song);
            Assert.Contains(expectedSong, saver.savedSongs());
        }

        [Fact]
        public void tryFindNullSongTest()
        {
            Assert.Throws<InvalidOperationException>(() => songsManager.findSong(title));
        }

        [Fact]
        public async Task deleteSongTest()
        {
            songsManager.addLocalSong(title, file, localPath);

            await songsManager.deleteSongAsync(expectedSong);
            Assert.Throws<InvalidOperationException>(() => songsManager.findSong(title));
            Assert.DoesNotContain(expectedSong, saver.savedSongs());
            Assert.True(File.Exists(expectedSong.LocalPath + expectedSong.File));
        }

        [Fact]
        public async Task deleteSongLockedbyAnotherUserTest()
        {
            //GIVEN
            //A song added to the songsManager, we expect the song being added to the songstorage and
            //be saved. Then we lock the song by another user, we expect to have lock file in local and version workspace.
            songsManager.addLocalSong(title, file, localPath);
            Song? song = songsManager.findSong(title);
            Assert.Equal(expectedSong, song);
            Assert.Contains(expectedSong, saver.savedSongs());
            string Username = "Second User";
            string BandPassword = "12df546@";
            string BandName = "Clic5456";
            string BandEmail = "testdklsjfhg@yahoo.com";
            User user2 = new User(BandName, BandPassword, Username, BandEmail);
            await locker.lockSongAsync(expectedSong, user2);
            Assert.True(File.Exists(expectedSong.LocalPath + @"\.lock"));
            Assert.True(File.Exists(version.versionPath + expectedSong.LocalPath + @"\.lock"));

            //WHEN we want to delete the song
            await songsManager.deleteSongAsync(expectedSong);

            //THEN we expect the song being removed from song storage and save. We expect the song to be
            //unlocked, lock file not being removed from local and version workspace.
            Assert.Throws<InvalidOperationException>(() => songsManager.findSong(title));
            Assert.DoesNotContain(expectedSong, saver.savedSongs());
            Assert.True(File.Exists(expectedSong.LocalPath + expectedSong.File));
            Assert.True(File.Exists(expectedSong.LocalPath + @"\.lock"));
            Assert.True(File.Exists(version.versionPath + expectedSong.LocalPath + @"\.lock"));
        }

        [Fact]
        public async Task deleteSongLockedbyUserTest()
        {
            //GIVEN
            //A song added to the songsManager, we expect the song being added to the songstorage and
            //be saved. Then we lock the song, we expect to have lock file in local and version workspace.
            songsManager.addLocalSong(title, file, localPath);
            Song? song = songsManager.findSong(title);
            Assert.Equal(expectedSong, song);
            Assert.Contains(expectedSong, saver.savedSongs());
            await locker.lockSongAsync(expectedSong, user);
            Assert.True(File.Exists(expectedSong.LocalPath + @"\.lock"));
            Assert.True(File.Exists(version.versionPath + expectedSong.LocalPath + @"\.lock"));

            //WHEN we want to delete the song
            await songsManager.deleteSongAsync(expectedSong);

            //THEN we expect the song being removed from song storage and save. We expect the song to be
            //locked, lock file removed from local and version workspace.
            Assert.Throws<InvalidOperationException>(() => songsManager.findSong(title));
            Assert.DoesNotContain(expectedSong, saver.savedSongs());
            Assert.True(File.Exists(expectedSong.LocalPath + expectedSong.File));
            Assert.False(File.Exists(expectedSong.LocalPath + @"\.lock"));
            Assert.False(File.Exists(version.versionPath + expectedSong.LocalPath + expectedSong.File));
            Assert.False(File.Exists(version.versionPath + expectedSong.LocalPath + @"\.lock"));
        }

        [Fact]
        public async Task updateSongTest()
        {
            //Add song for synchronization
            songsManager.addLocalSong(title, file, localPath);
            //Simulate a change on version workspace
            Directory.CreateDirectory(version.versionPath + expectedSong.LocalPath);
            FileStream fileStream = File.Create(version.versionPath + expectedSong.LocalPath + "audio.wav");
            fileStream.Close();
            Assert.True(File.Exists(version.versionPath + expectedSong.LocalPath + "audio.wav"));
            Assert.False(File.Exists(expectedSong.LocalPath + "audio.wav"));

            string errorMessage = await songsManager.updateSongAsync(expectedSong);

            Assert.True(File.Exists(version.versionPath + expectedSong.LocalPath + "audio.wav"));
            Assert.True(File.Exists(expectedSong.LocalPath + "audio.wav"));
            Assert.Equal(string.Empty, errorMessage);
        }

        [Fact]
        public async Task updateSongLockedTest()
        {
            //Add song for synchronization
            songsManager.addLocalSong(title, file, localPath);
            //Simulate a change on version workspace
            Directory.CreateDirectory(version.versionPath + expectedSong.LocalPath);
            FileStream fileStream = File.Create(version.versionPath + expectedSong.LocalPath + ".lock");
            fileStream.Close();

            string errorMessage = await songsManager.updateSongAsync(expectedSong);

            Assert.Equal(SongStatus.State.locked, expectedSong.Status.state);
            Assert.True(File.Exists(expectedSong.LocalPath + ".lock"));
            Assert.Equal(string.Empty, errorMessage);
        }

        [Fact]
        public async Task TryUpdateSongWithWrongBandNameTest()
        {
            //Add song for synchronization
            songsManager.addLocalSong(title, file, localPath);
            //Simulate a change on version workspace
            Directory.CreateDirectory(version.versionPath + expectedSong.LocalPath);
            FileStream fileStream = File.Create(version.versionPath + expectedSong.LocalPath + "audio.wav");
            fileStream.Close();
            Assert.True(File.Exists(version.versionPath + expectedSong.LocalPath + "audio.wav"));
            Assert.False(File.Exists(expectedSong.LocalPath + "audio.wav"));

            user.BandName = "Wrong Username";

            string errorMessage = await songsManager.updateSongAsync(expectedSong);

            Assert.True(File.Exists(version.versionPath + expectedSong.LocalPath + "audio.wav"));
            Assert.False(File.Exists(expectedSong.LocalPath + "audio.wav"));
            Assert.Equal("Error Bad Credentials", errorMessage);
        }

        [Fact]
        public async Task TryUpdateSongWithWrongBandPasswordTest()
        {
            //Add song for synchronization
            songsManager.addLocalSong(title, file, localPath);
            //Simulate a change on version workspace
            Directory.CreateDirectory(version.versionPath + expectedSong.LocalPath);
            FileStream fileStream = File.Create(version.versionPath + expectedSong.LocalPath + "audio.wav");
            fileStream.Close();
            Assert.True(File.Exists(version.versionPath + expectedSong.LocalPath + "audio.wav"));
            Assert.False(File.Exists(expectedSong.LocalPath + "audio.wav"));

            user.BandPassword = "Wrong Password";

            string errorMessage = await songsManager.updateSongAsync(expectedSong);

            Assert.True(File.Exists(version.versionPath + expectedSong.LocalPath + "audio.wav"));
            Assert.False(File.Exists(expectedSong.LocalPath + "audio.wav"));
            Assert.Equal("Error Bad Credentials", errorMessage);
        }

        [Fact]
        public async Task revertSongTest()
        {
            //Add song for synchronization
            songsManager.addLocalSong(title, file, localPath);
            //Simulate Modifications in local workspace
            FileStream fileStream = File.Create(expectedSong.LocalPath + "audio1.wav");
            fileStream.Close();
            fileStream = File.Create(expectedSong.LocalPath + "audio2.wav");
            fileStream.Close();
            //Different Version in version workspace
            Directory.CreateDirectory(version.versionPath + expectedSong.LocalPath);
            fileStream = File.Create(version.versionPath + expectedSong.LocalPath + "audio3.wav");
            fileStream.Close();

            string errorMessage = await songsManager.revertSongAsync(expectedSong);

            Assert.True(File.Exists(expectedSong.LocalPath + "audio3.wav"));
            Assert.False(File.Exists(expectedSong.LocalPath + "audio1.wav"));
            Assert.False(File.Exists(expectedSong.LocalPath + "audio2.wav"));
            Assert.Equal(string.Empty, errorMessage);
        }

        [Fact]
        public async Task uploadSongTest()
        {
            //Add song for synchronization
            songsManager.addLocalSong(title, file, localPath);
            //Simulate Modifications in local workspace
            FileStream fileStream = File.Create(expectedSong.LocalPath + "audio1.wav");
            fileStream.Close();
            fileStream = File.Create(expectedSong.LocalPath + "audio2.wav");
            fileStream.Close();
            //Different Version in version workspace
            Directory.CreateDirectory(version.versionPath + expectedSong.LocalPath);
            fileStream = File.Create(version.versionPath + expectedSong.LocalPath + "audio3.wav");
            fileStream.Close();

            string errorMessage = await songsManager.uploadNewSongVersionAsync(expectedSong, "New Version", "No description", true, false, false);

            Assert.False(File.Exists(version.versionPath + expectedSong.LocalPath + "audio3.wav"));
            Assert.True(File.Exists(version.versionPath + expectedSong.LocalPath + "audio1.wav"));
            Assert.True(File.Exists(version.versionPath + expectedSong.LocalPath + "audio2.wav"));
            Assert.True(File.Exists(version.versionPath + expectedSong.LocalPath + expectedSong.File));
            Assert.Equal(string.Empty, errorMessage);
        }

        [Fact]
        public async Task openSongTest()
        {
            //Simulate a change on version workspace
            Directory.CreateDirectory(version.versionPath + expectedSong.LocalPath);

            (bool, string) errorMessage = await songsManager.openSongAsync(expectedSong);

            Assert.Equal(SongStatus.State.locked, expectedSong.Status.state);
            Assert.True(File.Exists(expectedSong.LocalPath + ".lock"));
            Assert.True(File.Exists(version.versionPath + expectedSong.LocalPath + ".lock"));
            Assert.True(errorMessage.Item1);
            Assert.Equal(string.Empty, errorMessage.Item2);
        }

        [Fact]
        public async Task tryOpenSongLockedTest()
        {
            //Simulate locked song on version workspace
            Directory.CreateDirectory(version.versionPath + expectedSong.LocalPath);
            FileStream fileStream = File.Create(version.versionPath + expectedSong.LocalPath + ".lock");
            fileStream.Close();

            (bool, string) errorMessage = await songsManager.openSongAsync(expectedSong);

            Assert.Equal(SongStatus.State.locked, expectedSong.Status.state);
            Assert.True(File.Exists(expectedSong.LocalPath + ".lock"));
            Assert.True(File.Exists(version.versionPath + expectedSong.LocalPath + ".lock"));
            Assert.False(errorMessage.Item1);
            Assert.Equal("Already Locked", errorMessage.Item2);
        }

        [Fact]
        public async Task currentVersionTest()
        {
            songsManager.addLocalSong(title, file, localPath);
            string titleChange = "New Version";
            string descriptionChange = "No description";
            string errorMessage = await songsManager.uploadNewSongVersionAsync(expectedSong, titleChange, descriptionChange, true, false, false);

            SongVersion currentVersion = await songsManager.currentVersionAsync(expectedSong);

            string expectedVersionDescription = titleChange + "\n\n" + descriptionChange;
            string expectedVersionNumber = "1.0.0";
            string expectedVersionAuthor = user.Username;
            Assert.Equal(expectedVersionDescription, currentVersion.Description);
            Assert.Equal(expectedVersionNumber, currentVersion.Number);
            Assert.Equal(expectedVersionAuthor, currentVersion.Author);
        }

        [Theory]
        [InlineData(true, false, false, "1.0.0")]
        [InlineData(false, true, false, "0.1.0")]
        [InlineData(false, false, true, "0.0.1")]
        [InlineData(true, true, true, "1.1.1")]
        public async Task initialVersionNumberTest(bool compo, bool mix, bool mastering, string expectedVersionNumber)
        {
            title = "End of the Road";
            file = "test.song";
            localPath = "User/test/End of the Road/";
            songsManager.addLocalSong(title, file, localPath);
            string titleChange = "New Version";
            string descriptionChange = "No description";
            string errorMessage = await songsManager.uploadNewSongVersionAsync(expectedSong, titleChange, descriptionChange, compo, mix, mastering);

            SongVersion currentVersion = await songsManager.currentVersionAsync(expectedSong);

            Assert.Equal(expectedVersionNumber, currentVersion.Number);
        }

        [Theory]
        [InlineData(false, false, false, "1.1.1")]
        [InlineData(true, false, false, "2.0.0")]
        [InlineData(false, true, false, "1.2.0")]
        [InlineData(false, false, true, "1.1.2")]
        [InlineData(true, false, true, "2.0.1")]
        [InlineData(false, true, true, "1.2.1")]
        [InlineData(true, true, false, "2.1.0")]
        [InlineData(true, true, true, "2.1.1")]
        public async Task versionNumberTest(bool compo, bool mix, bool mastering, string expectedVersionNumber)
        {
            title = "End of the Road";
            file = "test.song";
            localPath = "User/test/End of the Road/";
            songsManager.addLocalSong(title, file, localPath);
            string titleChange = "New Version";
            string descriptionChange = "No description";
            //Simulate first upload by another user version 1.1.1
            string errorMessage = await songsManager.uploadNewSongVersionAsync(expectedSong, titleChange, descriptionChange, true, true, true);

            errorMessage = await songsManager.uploadNewSongVersionAsync(expectedSong, titleChange, descriptionChange, compo, mix, mastering);

            SongVersion currentVersion = await songsManager.currentVersionAsync(expectedSong);

            Assert.Equal(expectedVersionNumber, currentVersion.Number);
        }

        [Theory]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/", true, true, true)]
        public async Task versionsTest(string title, string file, string localPath, bool compo, bool mix, bool mastering)
        {
            songsManager.addLocalSong(title, file, localPath);
            string titleChange = "New Version";
            string descriptionChange = "No description";
            //Simulate uploads by another user
            string errorMessage = await songsManager.uploadNewSongVersionAsync(expectedSong, titleChange, descriptionChange, compo, mix, mastering);
            errorMessage = await songsManager.uploadNewSongVersionAsync(expectedSong, titleChange, descriptionChange, compo, mix, mastering);

            List<SongVersion> versions = await songsManager.versionsAsync(expectedSong);

            SongVersion expectedSongVersion = new SongVersion("1.1.1", titleChange + "\n\n" + descriptionChange, user.Username);
            SongVersion expectedSongVersion2 = new SongVersion("2.1.1", titleChange + "\n\n" + descriptionChange, user.Username);
            Assert.Contains(expectedSongVersion, versions);
            Assert.Contains(expectedSongVersion2, versions);
        }

        [Theory]
        [InlineData("End of the Road", "http://test.com/band/end-of-the-road", @"./SongsManagerTest")]
        public async Task addSharedSongTest(string songTitle, string sharedLink, string downloadPath)
        {
            Mock<IFileManager> fileManagerMock = new Mock<IFileManager>();
            fileManagerMock.Setup(m => m.findFileNameBasedOnExtensionAsync(downloadPath + @"\" + songTitle, ".song")).Returns(Task.FromResult("file.song"));
            SongsManager songsManagerTest = new SongsManager(version, saver, fileManagerMock.Object);

            await songsManagerTest.addSharedSongAsync(songTitle, sharedLink, downloadPath);

            //We expect a songVersioned created with the title
            Song expectedSong = new Song(songTitle, "file.song", downloadPath + @"\" + songTitle);
            Song song = songsManagerTest.findSong(songTitle);
            Assert.Equal(expectedSong, song);
            Assert.Contains(expectedSong, saver.savedSongs());
        }

        [Theory]
        [InlineData("End of the Road", "http://test.com/band/end-of-the-road", @"./SongsManagerTest")]
        public async Task addSharedSongErrorDownloadTest(string songTitle, string sharedLink, string downloadPath)
        {

            Mock<IVersionTool> versionToolMock = new Mock<IVersionTool>();
            versionToolMock.Setup(m => m.downloadSharedSongAsync(songTitle, sharedLink, downloadPath)).Returns(Task.FromResult("Error"));
            SongsManager songsManagerTest = new SongsManager(versionToolMock.Object, saver, fileManager.Object);

            string errorMessage = await songsManagerTest.addSharedSongAsync(songTitle, sharedLink, downloadPath);

            //We expect a songVersioned created with the title
            Song expectedSong = new Song(songTitle, "file.song", downloadPath + @"\" + songTitle);
            Assert.DoesNotContain(expectedSong, songsManagerTest.SongList);
            Assert.Equal("Error", errorMessage);
            //We expect to have called the addSharedSongAsync method in the songsManager
            versionToolMock.Verify(m => m.downloadSharedSongAsync(songTitle, sharedLink, downloadPath), Times.Once());
            Assert.DoesNotContain(expectedSong, saver.savedSongs());
        }

        [Theory]
        [InlineData("End of the Road", "http://test.com/band/end-of-the-road", @"./SongsManagerTest")]
        public async Task addSharedSongErrorFileNotFoundTest(string songTitle, string sharedLink, string downloadPath)
        {
            Mock<IFileManager> fileManagerMock = new Mock<IFileManager>();
            fileManagerMock.Setup(m => m.findFileNameBasedOnExtensionAsync(downloadPath + @"\" + songTitle, ".song")).Returns(Task.FromResult(string.Empty));
            SongsManager songsManagerTest = new SongsManager(version, saver, fileManagerMock.Object);

            string errorMessage = await songsManagerTest.addSharedSongAsync(songTitle, sharedLink, downloadPath);

            //We expect a songVersioned created with the title
            string localPath = downloadPath + @"\" + songTitle;
            Song expectedSong = new Song(songTitle, "file.song", localPath);
            Assert.DoesNotContain(expectedSong, songsManagerTest.SongList);
            fileManagerMock.Verify(m => m.findFileNameBasedOnExtensionAsync(localPath,".song"), Times.Once());
            Assert.Equal("Song File not Found in " + localPath, errorMessage);
        }


        [Theory]
        [InlineData("End of the Road", "test.song", "User/test/End of the Road/")]
        public async Task getShareLinkTest(string title, string file, string localPath)
        {
            Song song = new Song(title, file, localPath);
            Mock<IVersionTool> versionToolMock = new Mock<IVersionTool>();
            versionToolMock.Setup(m => m.shareSongAsync(song)).Returns(Task.FromResult("https://www.gitlab.com/end-of-the-road"));
            Mock<IFileManager> fileManagerMock = new Mock<IFileManager>();
            fileManagerMock.Setup(m => m.findFileNameBasedOnExtensionAsync(localPath + @"\" + title, ".song")).Returns(Task.FromResult(file));

            SongsManager songsManagerTest = new SongsManager(versionToolMock.Object, saver, fileManagerMock.Object);

            string errorMessage = await songsManagerTest.shareSongAsync(song);

            //We expect to have called the addSharedSongAsync method in the songsManager
            versionToolMock.Verify(m => m.shareSongAsync(song), Times.Once());

        }
    }
}
