using System;

namespace App1.Models
{
    public sealed class User
    {
        public User()
        {
            this.BandName = string.Empty;
            this.BandPassword = string.Empty;
            this.Username = string.Empty;
            this.BandEmail = string.Empty;
        }

        public User(string BandName, string BandPassword, string Username, string BandEmail)
        {
            this.BandName = BandName;
            this.BandPassword = BandPassword;
            this.Username = Username;
            this.BandEmail = BandEmail;
        }

        public override bool Equals(object? obj)
        {
            var song = obj as User;
            if (song == null)
                return false;
            if (this.BandName != song.BandName ||
               this.BandPassword != song.BandPassword ||
               this.Username != song.Username ||
               this.BandEmail != song.BandEmail)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.BandName, this.BandPassword, this.Username, this.BandEmail);
        }

        public string BandName { get; set; }
        public string BandEmail { get; set; }
        public string BandPassword { get; set; }
        public string Username { get; set; }
        
    }
}



