using App1.Models;
using App1.Models.Ports;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App1Tests.Mock
{
    public class VersioningMock : IVersionTool
    {
        public VersioningMock(User user) {
            this.user = user;
            string GitUsername1 = "Hear@fdjskjè_";
            string GitLabPassword1 = "12df546@";
            string GitLabUsername1 = "Clic5456";
            string GitEmail1 = "testdklsjfhg@yahoo.com";
            user1 = new User(GitLabUsername1, GitLabPassword1, GitUsername1, GitEmail1); ;
            string GitUsername2 = "Lithorama52";
            string GitLabPassword2 = "15@^_usnjdfb@";
            string GitLabUsername2 = "Erratum12";
            string GitEmail2 = "erratum12@gmail.com";
            user2 = new User(GitLabUsername2, GitLabPassword2, GitUsername2, GitEmail2);
        }

        public async Task<string> uploadSongAsync(Song song, string title, string description)
        {
            string errorMessage = await UserErrorAsync();
            return errorMessage;
        }

        public async Task<string> updateSongAsync(Song song)
        {
            string errorMessage = await UserErrorAsync();
            return errorMessage;
        }

        public async Task<string> revertSongAsync(Song song)
        {
            string errorMessage = await UserErrorAsync();
            return errorMessage;
        }

        private async Task<string> UserErrorAsync()
        {
            string errorMessage = await Task.Run(() =>
            {
                if (userIsDifferentFrom(user1) && 
                    userIsDifferentFrom(user2))
                    return "Error Bad Credentials";
                else
                    return string.Empty;
            });
            return errorMessage;
        }
        
        private bool userIsDifferentFrom(User expectedUser)
        {
            if (user == null || 
                user.GitLabUsername != expectedUser.GitLabUsername ||
                user.GitLabPassword != expectedUser.GitLabPassword ||
                user.GitUsername != expectedUser.GitUsername ||
                user.GitEmail != expectedUser.GitEmail)
                return true;
            return false;
        }

        public User? user { get; set; }
        private User user1;
        private User user2;
    }
}
