using App1.Models;
using App1.Models.Ports;
using App1.ViewModels;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinUIApp;
using Xunit;

namespace ViewModelsTests.SettingsPageViewModelTests
{
    public class SettingsPageViewModelTests
    {

        [Fact]
        public void initializedSettingsTest()
        {
            //Setup
            string BandName = "BandName";
            string BandPassword = "BandPassword";
            string Username = "Username";
            string BandEmail = "BandEmail@gmail.com";
            string expectedMusicSyncFolder = @"./SongsManagerTest/End of the Road";
            User user = new User(BandName, BandPassword, Username, BandEmail);
            UserViewModel expectedUserViewModel = new UserViewModel(user);
            Mock<ISaver> saverMock = new Mock<ISaver>();
            saverMock.Setup(m => m.savedMusicSyncFolder()).Returns(expectedMusicSyncFolder);
            saverMock.Setup(m => m.savedUser()).Returns(user);

            IFileManager fileManager = new FileManager();
            SettingsPageViewModel viewModel = new SettingsPageViewModel(saverMock.Object, fileManager);

            saverMock.Verify(m => m.savedMusicSyncFolder(), Times.Once());
            saverMock.Verify(m => m.savedUser(), Times.Once());
            Assert.Equal(expectedUserViewModel, viewModel.User);
            Assert.Equal(expectedMusicSyncFolder, viewModel.MusicSyncFolder);
        }

        [Fact]
        public void saveSettingsTest()
        {
            //Setup
            string BandName = "BandName";
            string BandPassword = "BandPassword";
            string Username = "Username";
            string BandEmail = "BandEmail@gmail.com";
            string expectedMusicSyncFolder = @"./SongsManagerTest/End of the Road";
            User user = new User(BandName, BandPassword, Username, BandEmail);
            UserViewModel expectedUserViewModel = new UserViewModel(user);
            Mock<ISaver> saverMock = new Mock<ISaver>();
            saverMock.Setup(m => m.savedMusicSyncFolder()).Returns(expectedMusicSyncFolder);
            saverMock.Setup(m => m.savedUser()).Returns(user);
            IFileManager fileManager = new FileManager();
            SettingsPageViewModel viewModel = new SettingsPageViewModel(saverMock.Object, fileManager);

            BandName = "BandNameChanged";
            BandPassword = "BandPasswordChanged";
            Username = "UsernameChanged";
            BandEmail = "BandEmailChanged@gmail.com";
            expectedMusicSyncFolder = @"./SongsManagerTest/End of the Road/Changed";
            user = new User(BandName, BandPassword, Username, BandEmail);

            viewModel.saveSettings(BandName, BandPassword, Username, BandEmail, expectedMusicSyncFolder);

            saverMock.Verify(m => m.saveSettings(user, expectedMusicSyncFolder), Times.Once());
        }
    }
}