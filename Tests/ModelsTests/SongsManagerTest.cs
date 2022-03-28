using App1.Models;
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
            expectedSong = new Song(title,file,localPath);

            version = new VersioningMock(user);
            saver = new SaverMock();
            saver.saveUser(user);
            fileManager = new FileManagerMock();
            songsManager = new SongsManager(version, saver, fileManager);
            locker = new Locker(version);
        }

        public void Dispose()
        {
            if(expectedSong.LocalPath != null)
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
        public IFileManager fileManager;
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
        public async Task updateAllSongsTest()
        {
            string title1 = "title1";
            string file1 = "file1.song";
            string localPath1 = @"./SongsManagerTest/song1/";
            Directory.CreateDirectory(localPath1);
            //FileStream fileStream1 = File.Create(localPath1 + file1);
            //fileStream1.Close();
            Song song1 = new Song(title1, file1, localPath1);

            string title2 = "title2";
            string file2 = "file2.song";
            string localPath2 = @"./SongsManagerTest/song2/";
            Directory.CreateDirectory(localPath2);
            //FileStream fileStream2 = File.Create(localPath2 + file1);
            //fileStream2.Close();
            Song song2 = new Song(title2, file2, localPath2);

            //Add song for synchronization
            songsManager.addLocalSong(title1, file1, localPath1);
            songsManager.addLocalSong(title2, file2, localPath2);
            //Simulate a change on song 1 version workspace
            Directory.CreateDirectory(version.versionPath + song1.LocalPath);
            FileStream fileStream = File.Create(version.versionPath + song1.LocalPath + "audio1.wav");
            fileStream.Close();
            Assert.True(File.Exists(version.versionPath + song1.LocalPath + "audio1.wav"));
            Assert.False(File.Exists(song1.LocalPath + "audio1.wav"));

            //Simulate a change on song 2 version workspace
            Directory.CreateDirectory(version.versionPath + song2.LocalPath);
            fileStream = File.Create(version.versionPath + song2.LocalPath + "audio2.wav");
            fileStream.Close();
            Assert.True(File.Exists(version.versionPath + song2.LocalPath + "audio2.wav"));
            Assert.False(File.Exists(song2.LocalPath + "audio2.wav"));

            string errorMessage = await songsManager.updateAllSongsAsync();

            Assert.True(File.Exists(song1.LocalPath + "audio1.wav"));
            Assert.True(File.Exists(song2.LocalPath + "audio2.wav"));
            Assert.Equal(string.Empty, errorMessage);
            if (song1.LocalPath != null)
            {
                Directory.Delete(song1.LocalPath, true);
            }
            if (song2.LocalPath != null)
            {
                Directory.Delete(song2.LocalPath, true);
            }
            Directory.Delete(version.versionPath + song1.LocalPath, true);
            Directory.Delete(version.versionPath + song2.LocalPath, true);
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
        [InlineData( true, false, false, "1.0.0")]
        [InlineData( false, true, false, "0.1.0")]
        [InlineData( false, false, true, "0.0.1")]
        [InlineData( true, true, true, "1.1.1")]
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
            await songsManager.addSharedSongAsync(songTitle, sharedLink, downloadPath);

            //We expect a songVersioned created with the title
            Song expectedSong = new Song(songTitle, "file.song", downloadPath + @"\" + songTitle);
            Song song = songsManager.findSong(songTitle);
            Assert.Equal(expectedSong, song);
            Assert.Contains(expectedSong, saver.savedSongs());
        }

        [Theory]
        [InlineData("End of the Road", "http://test.com/band/end-of-the-road", @"./SongsManagerTest")]
        public async Task addSharedSongErrorTest(string songTitle, string sharedLink, string downloadPath)
        {

            Mock<IVersionTool> versionToolMock = new Mock<IVersionTool>();
            versionToolMock.Setup(m => m.downloadSharedSongAsync(sharedLink, downloadPath + @"\" + songTitle)).Returns(Task.FromResult("Error"));
            SongsManager songsManagerTest = new SongsManager(versionToolMock.Object, saver, fileManager);

            string errorMessage = await songsManagerTest.addSharedSongAsync(songTitle, sharedLink, downloadPath);

            //We expect a songVersioned created with the title
            Song expectedSong = new Song(songTitle,"file.song", downloadPath + @"\" + songTitle);
            Assert.DoesNotContain(expectedSong, songsManagerTest.SongList);
            Assert.Equal("Error", errorMessage);
            //We expect to have called the addSharedSongAsync method in the songsManager
            versionToolMock.Verify(m => m.downloadSharedSongAsync(sharedLink, downloadPath + @"\" + songTitle), Times.Once());
            Assert.DoesNotContain(expectedSong, saver.savedSongs());
        }
    }
}
