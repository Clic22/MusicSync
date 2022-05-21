using App1.Models;
using App1.Models.Ports;
using WinUIApp;

namespace App1.ViewModels
{
    public class SettingsPageViewModel : Bindable
    {
        public SettingsPageViewModel(ISaver saver, IFileManager fileManager)
        {
            this._saver = saver;
            this._fileManager = fileManager;
            User = new UserViewModel(saver.SavedUser());
            _musicSyncFolder = saver.SavedMusicSyncFolder();
        }

        public void saveSettings(string BandName, string BandPassword, string Username, string BandEmail)
        {
            User userModel = new User(BandName,BandPassword,Username,BandEmail);
            _saver.SaveSettings(userModel, MusicSyncFolder);
        }

        public UserViewModel User { get; set; }
        private string _musicSyncFolder;
        public string MusicSyncFolder
        {
            get
            {
                return _musicSyncFolder;
            }
            set
            {
                value = _fileManager.FormatPath(value);
                SetProperty(ref _musicSyncFolder, value);
            }
        }
        private readonly ISaver _saver;
        private readonly IFileManager _fileManager;
    }
}
