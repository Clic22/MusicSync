using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace App1.Models
{
    public class Song : INotifyPropertyChanged
    {
        public enum SongStatus
        {
            upToDate,
            locked
        }

        public Song()
        {
            status = SongStatus.upToDate;
        }

        public Song(string newTitle, string newFile, string newLocalPath)
        {
            title = newTitle;
            file = newFile;
            localPath = newLocalPath;
            status = SongStatus.upToDate;
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool isLocked()
        {
            if (status == SongStatus.locked)
                return true;
            else
                return false;
        }

        public string title { get; set; }
        public string file { get; set; }
        public string localPath { get; set; }
        private SongStatus status_;
        public SongStatus status
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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
