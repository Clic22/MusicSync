namespace App1.Models
{
    public class Song
    {
        public Song()
        {
            Title = string.Empty;
            File = string.Empty;
            LocalPath = string.Empty;
            Guid = Guid.NewGuid();
            Status = new SongStatus();
        }

        
        public Song(string newTitle, string newFile, string newLocalPath)
        {
            Title = newTitle;
            File = newFile;
            Guid = Guid.NewGuid();
            LocalPath = newLocalPath;
            Status = new SongStatus();
        }
        
        public Song(string newTitle, string newFile, string newLocalPath, string guid)
        {
            Title = newTitle;
            File = newFile;
            Guid = new Guid(guid);
            LocalPath = newLocalPath;
            Status = new SongStatus();
        }

        public override bool Equals(object? obj)
        {
            var song = obj as Song;
            if (song == null)
                return false;
            if (this.Guid != song.Guid)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Guid);
        }

        public string Title { get; set; }
        public string File { get; set; }
        public string LocalPath { get; set; }
        public SongStatus Status;
        public Guid Guid {get; set;}
    }
}
