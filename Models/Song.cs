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
            Status = SongStatus.upToDate;
        }

        public Song(string newTitle, string newFile, string newLocalPath)
        {
            Title = newTitle;
            File = newFile;
            LocalPath = newLocalPath;
            Status = SongStatus.upToDate;
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string? Title { get; set; }
        public string? File { get; set; }
        public string? LocalPath { get; set; }
        private SongStatus status_;
        public SongStatus Status
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

        public event PropertyChangedEventHandler? PropertyChanged;

        public override bool Equals(object obj)
        {
            var song = obj as Song;
            if (song == null)
                return false;
            if(this.Title != song.Title ||
               this.Status != song.Status ||
               this.LocalPath != song.LocalPath ||
               this.File != song.File)
            {
                return false;
            }
            return true;    
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Title, this.Status, this.LocalPath, this.File);
        }

    }
}
