using App1.Models;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace App1Tests.Models
{
    [TestClass]
    public class UserTest
    {
        [TestMethod]
        public void UserCreationTest()
        {
            User user = new User();
            Assert.IsNotNull(user);
            Assert.IsNull(user.GitUsername);
            Assert.IsNull(user.GitLabPassword);
            Assert.IsNull(user.GitEmail);
            Assert.IsNull(user.GitLabUsername);
        }

        [TestMethod]
        public void UserCreationWithParametersTest()
        {
            string expectedGitUsername = "Hear@fdjskjè_";
            string expectedGitLabPassword = "12df546@";
            string expectedGitLabUsername = "CLic5456";
            string expectedGitEmail = "testdklsjfhg@yahoo.com";

            User user = new User(expectedGitLabUsername, expectedGitLabPassword, expectedGitUsername, expectedGitEmail); ;
            Assert.IsNotNull(user);
            Assert.AreEqual(expectedGitUsername, user.GitUsername);
            Assert.AreEqual(expectedGitLabPassword, user.GitLabPassword);
            Assert.AreEqual(expectedGitEmail,user.GitEmail);
            Assert.AreEqual(expectedGitLabUsername,user.GitLabUsername);
        }

        [TestMethod]
        public void TwoEqualUsersTest()
        {
            string expectedGitUsername = "Hear@fdjskjè_";
            string expectedGitLabPassword = "12df546@";
            string expectedGitLabUsername = "CLic5456";
            string expectedGitEmail = "testdklsjfhg@yahoo.com";

            User user1 = new User(expectedGitLabUsername, expectedGitLabPassword, expectedGitUsername, expectedGitEmail); ;
            User user2 = new User(expectedGitLabUsername, expectedGitLabPassword, expectedGitUsername, expectedGitEmail); ;

            Assert.AreEqual(user1, user2);
        }
    }
}