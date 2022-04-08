using App1.Models;
using App1.Models.Ports;
using WinUIApp;

namespace App1.ViewModels
{
    public class SettingsPageViewModel : Bindable
    {
        public SettingsPageViewModel(ISaver saver, IFileManager fileManager)
        {
            this.saver = saver;
            this.fileManager = fileManager;
            User = new UserViewModel(saver.savedUser());
            musicSyncFolder_ = saver.savedMusicSyncFolder();
            checkUpdatesFrequency_ = saver.savedCheckUpdatesFrequency().ToString();
        }

        public void saveSettings(string BandName, string BandPassword, string Username, string BandEmail)
        {
            Settings settingsToBeSaved = new Settings();
            User userModel = new User(BandName,BandPassword,Username,BandEmail);
            settingsToBeSaved.User = userModel;
            settingsToBeSaved.MusicSyncFolder = MusicSyncFolder;
            settingsToBeSaved.CheckUpdatesFrequency = int.Parse(CheckUpdatesFrequency);
            saver.saveSettings(settingsToBeSaved);
        }

        public UserViewModel User { get; set; }
        private string musicSyncFolder_;
        public string MusicSyncFolder
        {
            get
            {
                return musicSyncFolder_;
            }
            set
            {
                value = fileManager.FormatPath(value);
                SetProperty(ref musicSyncFolder_, value);
            }
        }

        private string checkUpdatesFrequency_;
        public string CheckUpdatesFrequency
        {
            get
            {
                return checkUpdatesFrequency_;
            }
            set
            {
                SetProperty(ref checkUpdatesFrequency_, value);
            }
        }

        private readonly ISaver saver;
        private readonly IFileManager fileManager;
    }
}
