using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1
{
    public sealed class User
    {
        private static readonly Lazy<User> _lazy = new Lazy<User>(() => new User());

        public static User Instance { get { return _lazy.Value; } }

        private User()
        {
            localSettings_ = Windows.Storage.ApplicationData.Current.LocalSettings;
            gitLabUsername_ = localSettings_.Values["gitLabUsername"] as string;
            gitLabPassword_ = localSettings_.Values["gitLabPassword"] as string;
            gitUsername_ = localSettings_.Values["gitUsername"] as string;
            gitEmail_ = localSettings_.Values["gitEmail"] as string;
        }

        public void saveSettings(string gitLabUsername, string gitLabPassword, string gitUsername, string gitEmail)
        {
            this.gitLabUsername = gitLabUsername;
            this.gitLabPassword = gitLabPassword;
            this.gitUsername = gitUsername;
            this.gitEmail = gitEmail;
        }


        private string gitLabUsername_;
        public string gitLabUsername
        {
            get
            {
                return gitLabUsername_;
            }
            set
            {
                gitLabUsername_ = value;
                localSettings_.Values.Remove("gitLabUsername");
                localSettings_.Values.Add("gitLabUsername", value);
            }
        }

        private string gitLabPassword_;
        public string gitLabPassword
        {
            get
            {
                return gitLabPassword_;
            }
            set
            {
                gitLabPassword_ = value;
                localSettings_.Values.Remove("gitLabPassword");
                localSettings_.Values.Add("gitLabPassword", value);
            }
        }
        private string gitUsername_;
        public string gitUsername
        {
            get
            {
                return gitUsername_;
            }
            set
            {
                gitUsername_ = value;
                localSettings_.Values.Remove("gitUsername");
                localSettings_.Values.Add("gitUsername", value);
            }
        }
        private string gitEmail_;
        public string gitEmail
        {
            get
            {
                return gitEmail_;
            }
            set
            {
                gitEmail_ = value;
                localSettings_.Values.Remove("gitEmail");
                localSettings_.Values.Add("gitEmail",value);
            }
        }

        private Windows.Storage.ApplicationDataContainer localSettings_ = Windows.Storage.ApplicationData.Current.LocalSettings;
    }
}


