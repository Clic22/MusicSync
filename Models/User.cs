﻿using System;

namespace App1.Models
{
    public sealed class User
    {
        public User()
        {
            this.GitLabUsername = string.Empty;
            this.GitLabPassword = string.Empty;
            this.GitUsername = string.Empty;
            this.GitEmail = string.Empty;
        }

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

        public override bool Equals(object? obj)
        {
            var song = obj as User;
            if (song == null)
                return false;
            if (this.GitLabUsername != song.GitLabUsername ||
               this.GitLabPassword != song.GitLabPassword ||
               this.GitUsername != song.GitUsername ||
               this.GitEmail != song.GitEmail)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.GitLabUsername, this.GitLabPassword, this.GitUsername, this.GitEmail);
        }
    }
}



