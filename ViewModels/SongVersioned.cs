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
        public SongVersioned(string title)
        {
            title_ = title;
            status_ = string.Empty;
            versionDescritpion_ = string.Empty;
            versionNumber_ = string.Empty;
            isUpdatingSong_ = false;
            isUploadingSong_ = false;
            isLoading_ = false;
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private readonly string title_;
        public string Title
        {
            get
            {
                return title_;
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

        private string versionDescritpion_;
        public string VersionDescription
        {
            get
            {
                return versionDescritpion_;
            }
            set
            {
                versionDescritpion_ = value;
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

        private bool isUpdatingSong_;
        public bool IsUpdatingSong
        {
            get
            {
                return isUpdatingSong_;
            }
            set
            {
                if (value)
                {
                    Status = "Updating...";
                    NotifyPropertyChanged("Status");
                }
                IsLoading = value;
            }
        }

        private bool isUploadingSong_;
        public bool IsUploadingSong
        {
            get
            {
                return isUploadingSong_;
            }
            set
            {
                if (value)
                {
                    Status = "Uploading...";
                    NotifyPropertyChanged("Status");
                }
                IsLoading = value;
            }
        }

        private bool isLoading_;
        public bool IsLoading
        {
            get
            {
                return isLoading_;
            }
            set
            {
                isLoading_ = value;
                NotifyPropertyChanged();
            }
        }

        public override bool Equals(object? obj)
        {
            var song = obj as SongVersioned;
            if (song == null)
                return false;
            if (this.Title != song.Title)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Title);
        }
    }
}
