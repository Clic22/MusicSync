namespace App1.Models
{
    public sealed class User
    {
        public User() { }

        public User(string gitLabUsername, string gitLabPassword, string gitUsername, string gitEmail)
        {
            this.gitLabUsername = gitLabUsername;
            this.gitLabPassword = gitLabPassword;
            this.gitUsername = gitUsername;
            this.gitEmail = gitEmail;
        }

        public string gitLabUsername { get; set; }
        public string gitLabPassword { get; set; }
        public string gitUsername { get; set; }
        public string gitEmail { get; set; }
    }
}


