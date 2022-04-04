using App1.Models;

namespace App1.ViewModels
{
    public class UserViewModel : Bindable
    {
        public UserViewModel()
        {
            this.bandName_ = string.Empty;
            this.bandPassword_ = string.Empty;
            this.username_ = string.Empty;
            this.bandEmail_ = string.Empty;
        }

        public UserViewModel(User user)
        {
            this.bandName_ = user.BandName;
            this.bandPassword_ = user.BandPassword;
            this.username_ = user.Username;
            this.bandEmail_ = user.BandEmail;
        }

        public UserViewModel(string BandName, string BandPassword, string Username, string BandEmail)
        {
            this.bandName_ = BandName;
            this.bandPassword_ = BandPassword;
            this.username_ = Username;
            this.bandEmail_ = BandEmail;
        }

        public override bool Equals(object? obj)
        {
            var song = obj as UserViewModel;
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

        private string bandName_;
        public string BandName
        {
            get
            {
                return bandName_;
            }
            set
            {
                SetProperty(ref bandName_, value);
            }
        }
        private string bandEmail_;
        public string BandEmail
        {
            get
            {
                return bandEmail_;
            }
            set
            {
                SetProperty(ref bandEmail_, value);
            }
        }
        private string bandPassword_;
        public string BandPassword
        {
            get
            {
                return bandPassword_;
            }
            set
            {
                SetProperty(ref bandPassword_, value);
            }
        }
        private string username_;
        public string Username
        {
            get
            {
                return username_;
            }
            set
            {
                SetProperty(ref username_, value);
            }
        }
    }
}



