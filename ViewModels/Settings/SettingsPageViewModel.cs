using App1.Models;
using App1.Models.Ports;
using App1.ViewModels.Settings.Entities;
using WinUIApp;

namespace App1.ViewModels
{
    public class SettingsPageViewModel : Bindable
    {
        public SettingsPageViewModel(ISaver saver)
        {
            this._saver = saver;
            var User = new UserViewModel(saver.SavedUser());
            var musicSyncFolder = saver.SavedMusicSyncFolder();
            Settings = new SettingsViewModel(User, musicSyncFolder);
        }

        public void SaveSettings()
        {
            var user = new User(Settings.User.BandName, Settings.User.BandPassword, Settings.User.Username, Settings.User.BandEmail);
            _saver.SaveUser(user);
            _saver.SaveMusicSyncFolder(Settings.MusicSyncFolder);
        }

        public SettingsViewModel Settings { get; set; }

        private readonly ISaver _saver;
    }
}
