namespace App1.Models
{
    public class SongVersion
    {
        public SongVersion()
        {
            Number = string.Empty;
            Description = string.Empty;
            Author = string.Empty;
        }

        public SongVersion(string number, string description, string author)
        {
            Number = number;
            Description = description;
            Author = author;
        }

        public override bool Equals(object? obj)
        {
            var song = obj as SongVersion;
            if (song == null)
                return false;
            if (this.Number != song.Number ||
               this.Description != song.Description ||
               this.Author != song.Author)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Number, this.Description, this.Author);
        }

        public string Number { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }

    }
}
