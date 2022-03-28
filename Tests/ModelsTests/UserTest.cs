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
            Assert.Empty(user.Username);
            Assert.Empty(user.BandPassword);
            Assert.Empty(user.BandEmail);
            Assert.Empty(user.BandName);
        }

        [Fact]
        public void UserCreationWithParametersTest()
        {
            string expectedUsername = "Hear@fdjskjè_";
            string expectedBandPassword = "12df546@";
            string expectedBandName = "CLic5456";
            string expectedBandEmail = "testdklsjfhg@yahoo.com";

            User user = new User(expectedBandName, expectedBandPassword, expectedUsername, expectedBandEmail); ;
            Assert.NotNull(user);
            Assert.Equal(expectedUsername, user.Username);
            Assert.Equal(expectedBandPassword, user.BandPassword);
            Assert.Equal(expectedBandEmail, user.BandEmail);
            Assert.Equal(expectedBandName, user.BandName);
        }

        [Fact]
        public void TwoEqualUsersTest()
        {
            string expectedUsername = "Hear@fdjskjè_";
            string expectedBandPassword = "12df546@";
            string expectedBandName = "CLic5456";
            string expectedBandEmail = "testdklsjfhg@yahoo.com";

            User user1 = new User(expectedBandName, expectedBandPassword, expectedUsername, expectedBandEmail); ;
            User user2 = new User(expectedBandName, expectedBandPassword, expectedUsername, expectedBandEmail); ;

            Assert.Equal(user1, user2);
        }

        [Fact]
        public void TwoDifferentUsersTest()
        {
            string expectedUsername = "Hear@fdjskjè_";
            string expectedBandPassword = "12df546@";
            string expectedBandName = "CLic5456";
            string expectedBandEmail = "testdklsjfhg@yahoo.com";

            User user1 = new User(expectedBandName, expectedBandPassword, expectedUsername, expectedBandEmail); ;
            expectedUsername = "Hear@fdjskjè_74";
            expectedBandPassword = "12df54fgvh6@";
            expectedBandName = "CLic5456sd";
            expectedBandEmail = "testdqsdklsjfhg@yahoo.com";
            User user2 = new User(expectedBandName, expectedBandPassword, expectedUsername, expectedBandEmail); ;

            Assert.NotEqual(user1, user2);
        }

        [Fact]
        public void OneNullUsersTest()
        {
            string expectedUsername = "Hear@fdjskjè_";
            string expectedBandPassword = "12df546@";
            string expectedBandName = "CLic5456";
            string expectedBandEmail = "testdklsjfhg@yahoo.com";

            User user1 = new User(expectedBandName, expectedBandPassword, expectedUsername, expectedBandEmail); ;
            User? user2 = null;
            Assert.False(user1.Equals(user2));
        }
    }
}