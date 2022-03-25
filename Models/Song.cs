namespace App1.Models
{
    public class Song
    {
        public Song()
        {
            Title = string.Empty;
            File = string.Empty;
            LocalPath = string.Empty;
            Status = new SongStatus();
        }

        public Song(string newTitle, string newFile, string newLocalPath)
        {
            Title = newTitle;
            File = newFile;
            LocalPath = newLocalPath;
            Status = new SongStatus();
        }

        public override bool Equals(object? obj)
        {
            var song = obj as Song;
            if (song == null)
                return false;
            if (this.Title != song.Title ||
               this.LocalPath != song.LocalPath ||
               this.File != song.File)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Title, this.LocalPath, this.File);
        }

        public string Title { get; set; }
        public string File { get; set; }
        public string LocalPath { get; set; }
        public SongStatus Status;
    }
}
