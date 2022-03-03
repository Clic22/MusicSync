using System;

namespace App1.Models
{
    public sealed class User
    {
        public User() { }

        public User(string gitLabUsername, string gitLabPassword, string gitUsername, string gitEmail)
        {
            this.GitLabUsername = gitLabUsername;
            this.GitLabPassword = gitLabPassword;
            this.GitUsername = gitUsername;
            this.GitEmail = gitEmail;
        }

        public string? GitLabUsername { get; set; }
        public string? GitLabPassword { get; set; }
        public string? GitUsername { get; set; }
        public string? GitEmail { get; set; }
    }
}



