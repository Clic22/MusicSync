using App1.Models;

namespace App1.ViewModels
{
    public class UserViewModel : Bindable
    {
        public UserViewModel(User user)
        {
            this._bandName = user.BandName;
            this._bandPassword = user.BandPassword;
            this._username = user.Username;
            this._bandEmail = user.BandEmail;
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

        private string _bandName;
        public string BandName
        {
            get
            {
                return _bandName;
            }
            set
            {
                SetProperty(ref _bandName, value);
            }
        }
        private string _bandEmail;
        public string BandEmail
        {
            get
            {
                return _bandEmail;
            }
            set
            {
                SetProperty(ref _bandEmail, value);
            }
        }
        private string _bandPassword;
        public string BandPassword
        {
            get
            {
                return _bandPassword;
            }
            set
            {
                SetProperty(ref _bandPassword, value);
            }
        }
        private string _username;
        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                SetProperty(ref _username, value);
            }
        }
    }
}



