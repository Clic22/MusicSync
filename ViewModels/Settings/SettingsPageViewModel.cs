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
            User = new UserViewModel(saver.SavedUser());
            musicSyncFolder_ = saver.savedMusicSyncFolder();
        }

        public void saveSettings(string BandName, string BandPassword, string Username, string BandEmail)
        {
            User userModel = new User(BandName,BandPassword,Username,BandEmail);
            saver.saveSettings(userModel, MusicSyncFolder);
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
        private readonly ISaver saver;
        private readonly IFileManager fileManager;
    }
}
