using App1.Models;
using App1.Models.Ports;
using App1.ViewModels.Settings.Entities;
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

        public void SaveSettings()
        {
            var user = new User(User.BandName, User.BandPassword, User.Username, User.BandEmail);
            _saver.SaveUser(user);
            _saver.SaveMusicSyncFolder(MusicSyncFolder);
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
