using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace App1
{
    public class Song : INotifyPropertyChanged
    {
        public enum SongStatus
        {
            upToDate,
            locked,
            modifiedRef
        }

        public Song()
        {
            title = string.Empty;
            file = string.Empty;
            localPath = string.Empty;
            status = SongStatus.upToDate;
        }

        public Song(string newTitle, string newFile, string newLocalPath)
        {
            title = newTitle;
            file = newFile;
            localPath = newLocalPath;
            status = SongStatus.upToDate;
        }
 
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
