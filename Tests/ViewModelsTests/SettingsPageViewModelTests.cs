using App1.Models;
using App1.Models.Ports;
using App1.ViewModels;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WinUIApp;
using Xunit;

namespace ViewModelsTests.SettingsPageViewModelTests
{
    public class SettingsPageViewModelTests
    {

        [Fact]
        public void InitializedSettingsTest()
        {
            //Setup
            string BandName = "BandName";
            string BandPassword = "BandPassword";
            string Username = "Username";
            string BandEmail = "BandEmail@gmail.com";
            string expectedMusicSyncFolder = @"./SongsManagerTest/End of the Road/";
            var expectedUser = new User(BandName, BandPassword, Username, BandEmail);
            var expectedUserViewModel = new UserViewModel(expectedUser);
            var saverMock = new Mock<ISaver>();
            saverMock.Setup(m => m.SavedMusicSyncFolder()).Returns(expectedMusicSyncFolder);
            saverMock.Setup(m => m.SavedUser()).Returns(expectedUser);

            IFileManager fileManager = new FileManager();
            SettingsPageViewModel viewModel = new SettingsPageViewModel(saverMock.Object);

            saverMock.Verify(m => m.SavedMusicSyncFolder(), Times.Once());
            saverMock.Verify(m => m.SavedUser(), Times.Once());
            Assert.Equal(expectedUserViewModel, viewModel.Settings.User);
            Assert.Equal(expectedMusicSyncFolder, viewModel.Settings.MusicSyncFolder);
            
        }

        [Fact]
        public void ChangeSettingsTest()
        {
            //Setup
            string BandName = "BandName";
            string BandPassword = "BandPassword";
            string Username = "Username";
            string BandEmail = "BandEmail@gmail.com";
            string expectedMusicSyncFolder = @"./SongsManagerTest/End of the Road/";
            var expectedUser = new User(BandName, BandPassword, Username, BandEmail);
            var expectedUserViewModel = new UserViewModel(expectedUser);
            var saverMock = new Mock<ISaver>();
            saverMock.Setup(m => m.SavedMusicSyncFolder()).Returns(expectedMusicSyncFolder);
            saverMock.Setup(m => m.SavedUser()).Returns(expectedUser);

            IFileManager fileManager = new FileManager();
            SettingsPageViewModel viewModel = new SettingsPageViewModel(saverMock.Object);

            Action action = () => viewModel.Settings.User.Username = "New Username";
            Assert.PropertyChanged(viewModel.Settings.User, "Username", action);
            action = () => viewModel.Settings.User.BandName = "New BandName";
            Assert.PropertyChanged(viewModel.Settings.User, "BandName", action);
            action = () => viewModel.Settings.User.BandPassword = "New BandPassword";
            Assert.PropertyChanged(viewModel.Settings.User, "BandPassword", action);
            action = () => viewModel.Settings.User.BandEmail = "New BandEmail";
            Assert.PropertyChanged(viewModel.Settings.User, "BandEmail", action);

            action = () => viewModel.Settings.MusicSyncFolder = "New MusicSyncFolder Location";
            Assert.PropertyChanged(viewModel.Settings, "MusicSyncFolder", action);
        }

        [Fact]
        public void SaveSettingsTest()
        {
            //Setup
            string BandName = "BandName";
            string BandPassword = "BandPassword";
            string Username = "Username";
            string BandEmail = "BandEmail@gmail.com";
            string expectedMusicSyncFolder = @"./SongsManagerTest/End of the Road\";
            User user = new User(BandName, BandPassword, Username, BandEmail);
            UserViewModel expectedUserViewModel = new UserViewModel(user);
            Mock<ISaver> saverMock = new Mock<ISaver>();
            saverMock.Setup(m => m.SavedMusicSyncFolder()).Returns(expectedMusicSyncFolder);
            saverMock.Setup(m => m.SavedUser()).Returns(user);
            IFileManager fileManager = new FileManager();
            SettingsPageViewModel viewModel = new SettingsPageViewModel(saverMock.Object);

            BandName = "BandNameChanged";
            BandPassword = "BandPasswordChanged";
            Username = "UsernameChanged";
            BandEmail = "BandEmailChanged@gmail.com";
            expectedMusicSyncFolder = @"./SongsManagerTest/End of the Road/Changed";
            user = new User(BandName, BandPassword, Username, BandEmail);

            viewModel.Settings.MusicSyncFolder = expectedMusicSyncFolder;
            viewModel.Settings.User = new UserViewModel(user);

            viewModel.SaveSettings();

            saverMock.Verify(m => m.SaveUser(user), Times.Once());
            saverMock.Verify(m => m.SaveMusicSyncFolder(expectedMusicSyncFolder + Path.DirectorySeparatorChar), Times.Once());
        }
    }
}