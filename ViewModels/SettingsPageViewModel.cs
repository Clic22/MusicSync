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
            user = new UserViewModel(saver.savedUser());
            musicSyncFolder_ = saver.savedMusicSyncFolder();
        }

        public void saveSettings(string BandName, string BandPassword, string Username, string BandEmail, string musicSyncFolder)
        {
            User userModel = new User(BandName,BandPassword,Username,BandEmail);
            saver.saveSettings(userModel, musicSyncFolder);
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
