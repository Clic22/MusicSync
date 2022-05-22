using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1.ViewModels.Settings.Entities
{
    public class SettingsViewModel : Bindable   
    {
        public SettingsViewModel(UserViewModel user, string musicSyncFolder)
        {
            User = user;
            _musicSyncFolder = musicSyncFolder;
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
                SetProperty(ref _musicSyncFolder, value);
            }
        }


    }
}
