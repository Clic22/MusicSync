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
        }

        public void saveSettings(string BandName, string BandPassword, string Username, string BandEmail)
        {
            Settings settingsToBeSaved = new Settings();
            User userModel = new User(BandName,BandPassword,Username,BandEmail);
            settingsToBeSaved.User = userModel;
            settingsToBeSaved.MusicSyncFolder = MusicSyncFolder;
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

        private string checkUpdatesPeriod_;
        public string CheckUpdatesPeriod
        {
            get
            {
                return checkUpdatesPeriod_;
            }
            set
            {
                SetProperty(ref checkUpdatesPeriod_, value);
            }
        }

        private readonly ISaver saver;
        private readonly IFileManager fileManager;
    }
}
