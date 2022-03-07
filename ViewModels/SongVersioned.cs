using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace App1.ViewModels
{
    public class SongVersioned : INotifyPropertyChanged
    {
        public SongVersioned()
        {
            title_ = string.Empty;
            status_ = string.Empty;
            versionNumber_ = string.Empty;
            versionDescription_ = string.Empty;
        }

        public SongVersioned(string title)
        {
            title_ = title;
            status_ = "Up to Date";
            versionNumber_ = string.Empty;
            versionDescription_ = string.Empty;
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private string title_;
        public string Title
        {
            get
            {
                return title_;
            }
            set
            {
                title_ = value;
                NotifyPropertyChanged();
            }
        }
        private string status_;
        public string Status
        {
            get
            {
                return status_;
            }
            set
            {
                status_ = value;
                NotifyPropertyChanged();
            }
        }
        private string versionNumber_;
        public string VersionNumber
        {
            get
            {
                return versionNumber_;
            }
            set
            {
                versionNumber_ = value;
                NotifyPropertyChanged();
            }
        }
        private string versionDescription_;
        public string VersionDescription
        {
            get
            {
                return versionDescription_;
            }
            set
            {
                versionDescription_ = value;
                NotifyPropertyChanged();
            }
        }
    }
}
