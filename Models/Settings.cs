namespace App1.Models
{
    public class Settings
    {
        public Settings()
        {
            user_ = new User();
            musicSyncFolder_ = string.Empty;
            checkUpdatesFrequency_ = 0;
        }

        private User user_;
        public User User
        {
            get { return user_; }
            set { user_ = value; }
        }
        private string musicSyncFolder_;
        public string MusicSyncFolder
        {
            get { return musicSyncFolder_; }
            set { musicSyncFolder_ = value; }
        }
        private int checkUpdatesFrequency_;
        public int CheckUpdatesFrequency
        {
            get { return checkUpdatesFrequency_; }
            set { checkUpdatesFrequency_ = value; }
        }

        public override bool Equals(object? obj)
        {
            var settings = obj as Settings;
            if (settings == null)
                return false;
            if (this.User.BandName != settings.User.BandName ||
               this.User.BandPassword != settings.User.BandPassword ||
               this.User.Username != settings.User.Username ||
               this.User.BandEmail != settings.User.BandEmail ||
               this.MusicSyncFolder != settings.MusicSyncFolder ||
               this.CheckUpdatesFrequency != settings.CheckUpdatesFrequency)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.User.BandName, this.User.BandPassword, 
                                    this.User.Username, this.User.BandEmail, 
                                    this.MusicSyncFolder, this.CheckUpdatesFrequency);
        }
    }
}



