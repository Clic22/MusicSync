using App1.Models;
using Xunit;

namespace ModelsTests.UserTest
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

        [Fact]
        public void TwoEqualUsersTest()
        {
            string expectedGitUsername = "Hear@fdjskjè_";
            string expectedGitLabPassword = "12df546@";
            string expectedGitLabUsername = "CLic5456";
            string expectedGitEmail = "testdklsjfhg@yahoo.com";

            User user1 = new User(expectedGitLabUsername, expectedGitLabPassword, expectedGitUsername, expectedGitEmail); ;
            User user2 = new User(expectedGitLabUsername, expectedGitLabPassword, expectedGitUsername, expectedGitEmail); ;

            Assert.Equal(user1, user2);
        }

        [Fact]
        public void TwoDifferentUsersTest()
        {
            string expectedGitUsername = "Hear@fdjskjè_";
            string expectedGitLabPassword = "12df546@";
            string expectedGitLabUsername = "CLic5456";
            string expectedGitEmail = "testdklsjfhg@yahoo.com";

            User user1 = new User(expectedGitLabUsername, expectedGitLabPassword, expectedGitUsername, expectedGitEmail); ;
            expectedGitUsername = "Hear@fdjskjè_74";
            expectedGitLabPassword = "12df54fgvh6@";
            expectedGitLabUsername = "CLic5456sd";
            expectedGitEmail = "testdqsdklsjfhg@yahoo.com";
            User user2 = new User(expectedGitLabUsername, expectedGitLabPassword, expectedGitUsername, expectedGitEmail); ;

            Assert.NotEqual(user1, user2);
        }

        [Fact]
        public void OneNullUsersTest()
        {
            string expectedGitUsername = "Hear@fdjskjè_";
            string expectedGitLabPassword = "12df546@";
            string expectedGitLabUsername = "CLic5456";
            string expectedGitEmail = "testdklsjfhg@yahoo.com";

            User user1 = new User(expectedGitLabUsername, expectedGitLabPassword, expectedGitUsername, expectedGitEmail); ;
            User? user2 = null;
            user1.Equals(user2);
        }
    }
}