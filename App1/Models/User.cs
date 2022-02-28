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

        public string GitLabUsername { get; set; }
        public string GitLabPassword { get; set; }
        public string GitUsername { get; set; }
        public string GitEmail { get; set; }


        public bool Equals(User user)
        {
            if (user is null)
            {
                return false;
            }

            // Optimization for a common success case.
            if (object.ReferenceEquals(this, user))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != user.GetType())
            {
                return false;
            }

            // Return true if the fields match.
            // Note that the base class is not invoked because it is
            // System.Object, which defines Equals as reference equality.
            return (GitLabUsername == user.GitLabUsername) && (GitLabPassword == user.GitLabPassword) &&
                   (GitUsername == user.GitUsername) && (GitEmail == user.GitEmail);
        }

        public static bool operator ==(User user1, User user2)
        {
            if (user1 is null)
            {
                if (user2 is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return user1.Equals(user2);
        }

        public static bool operator !=(User lhs, User rhs) => !(lhs == rhs);
    }
}



