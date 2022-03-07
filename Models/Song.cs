namespace App1.Models
{
    public class Song
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

        public string? Title { get; set; }
        public string? File { get; set; }
        public string? LocalPath { get; set; }
        public string? VersionDescription { get; set; }
        public SongStatus Status;
    }
}
