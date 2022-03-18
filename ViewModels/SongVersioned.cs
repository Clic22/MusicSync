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
