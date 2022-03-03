using App1.Models;
using System;
using Xunit;

namespace App1Tests.Models
{
    public class UserTest
    {
        [Fact]
        public void UserCreationTest()
        {
            User user = new User();
            Assert.NotNull(user);
            Assert.Null(user.GitUsername);
            Assert.Null(user.GitLabPassword);
            Assert.Null(user.GitEmail);
            Assert.Null(user.GitLabUsername);
        }

        [Fact]
        public void UserCreationWithParametersTest()
        {
            string expectedGitUsername = "Hear@fdjskjè_";
            string expectedGitLabPassword = "12df546@";
            string expectedGitLabUsername = "CLic5456";
            string expectedGitEmail = "testdklsjfhg@yahoo.com";

            User user = new User(expectedGitLabUsername, expectedGitLabPassword, expectedGitUsername, expectedGitEmail); ;
            Assert.NotNull(user);
            Assert.Equal(expectedGitUsername, user.GitUsername);
            Assert.Equal(expectedGitLabPassword, user.GitLabPassword);
            Assert.Equal(expectedGitEmail,user.GitEmail);
            Assert.Equal(expectedGitLabUsername,user.GitLabUsername);
        }
    }
}