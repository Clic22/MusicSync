using App1.Models;
using App1.Models.Ports;
using App1Tests.Mock;
using System;
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
            string GitUsername = "Hear@fdjskjè_";
            string GitLabPassword = "12df546@";
            string GitLabUsername = "Clic5456";
            string GitEmail = "testdklsjfhg@yahoo.com";
            user = new User(GitLabUsername, GitLabPassword, GitUsername, GitEmail);

            title = "title";
            file = "file.song";
            localPath = @"./SongsManagerTest/End of the Road/";
            Directory.CreateDirectory(localPath);
            FileStream fileStream = File.Create(localPath + file);
            fileStream.Close();
            expectedSong = new Song(title,file,localPath);

            version = new VersioningMock(user);
            saver = new SaverMock();
            saver.saveUser(user);
            songsManager = new SongsManager(version, saver);
            locker = new Locker(version);
        }

        public void Dispose()
        {

            Directory.Delete(expectedSong.LocalPath, true);
            if (Directory.Exists(version.VersionPath + expectedSong.LocalPath))
            {
                Directory.Delete(version.VersionPath + expectedSong.LocalPath, true);
            }
            songsManager.SongList.Clear();
        }

        public User user;
        public Song expectedSong;
        public ISaver saver;
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
        public void addSongTest()
        {
            songsManager.addSong(title,file,localPath);

            Assert.Contains(expectedSong, songsManager.SongList);
            Assert.Contains(expectedSong, saver.savedSongs());
        }

        [Fact]
        public async Task deleteSongTest()
        {
            songsManager.addSong(title, file, localPath);

            await songsManager.deleteSong(expectedSong);

            Assert.DoesNotContain(expectedSong, songsManager.SongList);
            Assert.DoesNotContain(expectedSong, saver.savedSongs());
            Assert.True(File.Exists(expectedSong.LocalPath + expectedSong.File));
        }

        [Fact]
        public async Task deleteSongLockedbyAnotherUserTest()
        {
            //GIVEN
            //A song added to the songsManager, we expect the song being added to the songstorage and
            //be saved. Then we lock the song by another user, we expect to have lock file in local and version workspace.
            songsManager.addSong(title, file, localPath);
            Assert.Contains(expectedSong, songsManager.SongList);
            Assert.Contains(expectedSong, saver.savedSongs());
            string GitUsername = "Second User";
            string GitLabPassword = "12df546@";
            string GitLabUsername = "Clic5456";
            string GitEmail = "testdklsjfhg@yahoo.com";
            User user2 = new User(GitLabUsername, GitLabPassword, GitUsername, GitEmail);
            await locker.lockSongAsync(expectedSong, user2);
            Assert.True(File.Exists(expectedSong.LocalPath + @"\.lock"));
            Assert.True(File.Exists(version.VersionPath + expectedSong.LocalPath + @"\.lock"));

            //WHEN we want to delete the song
            await songsManager.deleteSong(expectedSong);

            //THEN we expect the song being removed from song storage and save. We expect the song to be
            //unlocked, lock file not being removed from local and version workspace.
            Assert.DoesNotContain(expectedSong, songsManager.SongList);
            Assert.DoesNotContain(expectedSong, saver.savedSongs());
            Assert.True(File.Exists(expectedSong.LocalPath + expectedSong.File));
            Assert.True(File.Exists(expectedSong.LocalPath + @"\.lock"));
            Assert.True(File.Exists(version.VersionPath + expectedSong.LocalPath + @"\.lock"));
        }

        [Fact]
        public async Task deleteSongLockedbyUserTest()
        {
            //GIVEN
            //A song added to the songsManager, we expect the song being added to the songstorage and
            //be saved. Then we lock the song, we expect to have lock file in local and version workspace.
            songsManager.addSong(title, file, localPath);
            Assert.Contains(expectedSong, songsManager.SongList);
            Assert.Contains(expectedSong, saver.savedSongs());
            await locker.lockSongAsync(expectedSong, user);
            Assert.True(File.Exists(expectedSong.LocalPath + @"\.lock"));
            Assert.True(File.Exists(version.VersionPath + expectedSong.LocalPath + @"\.lock"));

            //WHEN we want to delete the song
            await songsManager.deleteSong(expectedSong);

            //THEN we expect the song being removed from song storage and save. We expect the song to be
            //locked, lock file removed from local and version workspace.
            Assert.DoesNotContain(expectedSong, songsManager.SongList);
            Assert.DoesNotContain(expectedSong, saver.savedSongs());
            Assert.True(File.Exists(expectedSong.LocalPath + expectedSong.File));
            Assert.False(File.Exists(expectedSong.LocalPath + @"\.lock"));
            Assert.True(File.Exists(version.VersionPath + expectedSong.LocalPath + expectedSong.File));
            Assert.False(File.Exists(version.VersionPath + expectedSong.LocalPath + @"\.lock"));
        }

        [Fact]
        public async Task updateSongTest()
        {
            //Add song for synchronization
            songsManager.addSong(title, file, localPath);
            //Simulate a change on version workspace
            Directory.CreateDirectory(version.VersionPath + expectedSong.LocalPath);
            FileStream fileStream = File.Create(version.VersionPath + expectedSong.LocalPath + "audio.wav");
            fileStream.Close();
            Assert.True(File.Exists(version.VersionPath + expectedSong.LocalPath + "audio.wav"));
            Assert.False(File.Exists(expectedSong.LocalPath + "audio.wav"));

            string errorMessage = await songsManager.updateSongAsync(expectedSong);
            Assert.True(File.Exists(version.VersionPath + expectedSong.LocalPath + "audio.wav"));
            Assert.True(File.Exists(expectedSong.LocalPath + "audio.wav"));
            Assert.Equal(string.Empty, errorMessage);
        }

    }

}
