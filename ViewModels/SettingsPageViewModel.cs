using App1.Models;
using App1.Models.Ports;
using WinUIApp;

namespace App1.ViewModels
{
    public class SettingsPageViewModel : Bindable
    {
        public SettingsPageViewModel()
        {
            saver = new LocalSettingsSaver();
            fileManager = new FileManager();
            user = new UserViewModel(saver.savedUser());
            musicSyncFolder_ = saver.savedMusicSyncFolder();
        }

        public void saveSettings()
        {
            User userModel = new User(user.BandName,user.BandPassword,user.Username,user.BandEmail);
            saver.saveSettings(userModel, musicSyncFolder_);
        }

        public UserViewModel user;
        private string musicSyncFolder_;
        public string MusicSyncFolder
        {
            get
            {
                return musicSyncFolder_;
            }
            set
            {
                fileManager.FormatPath(ref value);
                SetProperty(ref musicSyncFolder_, value);
            }
        }
        private ISaver saver;
        private IFileManager fileManager;
    }
}
