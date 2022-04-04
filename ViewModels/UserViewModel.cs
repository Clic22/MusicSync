using App1.Models;

namespace App1.ViewModels
{
    public class UserViewModel : Bindable
    {
        public UserViewModel(User user)
        {
            this.bandName_ = user.BandName;
            this.bandPassword_ = user.BandPassword;
            this.username_ = user.Username;
            this.bandEmail_ = user.BandEmail;
        }

        public override bool Equals(object? obj)
        {
            var user = obj as UserViewModel;
            if (user == null)
                return false;
            if (this.BandName != user.BandName ||
               this.BandPassword != user.BandPassword ||
               this.Username != user.Username ||
               this.BandEmail != user.BandEmail)
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
        }
        private string bandEmail_;
        public string BandEmail
        {
            get
            {
                return bandEmail_;
            }
        }
        private string bandPassword_;
        public string BandPassword
        {
            get
            {
                return bandPassword_;
            }
        }
        private string username_;
        public string Username
        {
            get
            {
                return username_;
            }
        }
    }
}



