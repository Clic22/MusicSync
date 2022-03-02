using App1.Adapters;
using App1.Models;
using App1.Models.Ports;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace AdaptersTests
{
    [TestClass]
    public class LocalSettingsSaverTest
    {
        [TestMethod]
        public void SaveUserTest()
        {
            string expectedGitUsername = "Hear@fdjskjè_";
            string expectedGitLabPassword = "12df546@";
            string expectedGitLabUsername = "CLic5456";
            string expectedGitEmail = "testdklsjfhg@yahoo.com";

            User expectedUser = new User(expectedGitLabUsername, expectedGitLabPassword, expectedGitUsername, expectedGitEmail);
            ISaver saver = new LocalSettingsSaver();
            saver.saveUser(expectedUser);

            User userSaved = saver.savedUser();
            Assert.AreEqual(expectedUser.GitLabUsername, userSaved.GitLabUsername);
            Assert.AreEqual(expectedUser.GitLabPassword, userSaved.GitLabPassword);
            Assert.AreEqual(expectedUser.GitEmail, userSaved.GitEmail);
            Assert.AreEqual(expectedUser.GitUsername, userSaved.GitUsername);
        }
    }
}