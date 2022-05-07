namespace App1.Models
{
    public class SongVersion
    {
        public SongVersion()
        {
            Number = string.Empty;
            Description = string.Empty;
            Author = string.Empty;
            Date = DateOnly.MinValue;
        }

        public SongVersion(string number, string description, string author, DateOnly date)
        {
            Number = number;
            Description = description;
            Author = author;
            Date = date;
        }

        public override bool Equals(object? obj)
        {
            var version = obj as SongVersion;
            if (version == null)
                return false;
            if (this.Number != version.Number ||
               this.Description != version.Description ||
               this.Author != version.Author ||
               this.Date != version.Date)
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
        public DateOnly Date { get; set; }

    }
}
