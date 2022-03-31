using App1.Models;
using App1.Models.Ports;
using App1Tests.Mock;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace ModelsTests.VersionToolTest
{
    public class VersionToolTest
    {
        [Theory]
        [InlineData(true, false, false, "1.0.0")]
        [InlineData(false, true, false, "0.1.0")]
        [InlineData(false, false, true, "0.0.1")]
        [InlineData(true, true, true, "1.1.1")]
        public async Task initialVersionNumberTest(bool compo, bool mix, bool mastering, string expectedVersionNumber)
        {
            User user = new User();
            IVersionTool versionTool = new VersioningMock(user);
            Song song = new Song();
            string versionNumber = await versionTool.newVersionNumberAsync(song, compo, mix, mastering);

            Assert.Equal(expectedVersionNumber, versionNumber);
        }

        [Theory]
        [InlineData(true, false, false, "2.0.0")]
        [InlineData(false, true, false, "1.2.0")]
        [InlineData(false, false, true, "1.1.2")]
        [InlineData(true, false, true, "2.0.1")]
        [InlineData(false, true, true, "1.2.1")]
        [InlineData(true, true, false, "2.1.0")]
        [InlineData(true, true, true, "2.1.1")]
        public async Task versionNumberTest(bool compo, bool mix, bool mastering, string expectedVersionNumber)
        {
            User user = new User();
            Song song = new Song();
            Mock<IVersionTool> versionToolMock = new Mock<IVersionTool>();
            SongVersion initialVersion = new SongVersion();
            initialVersion.Number = "1.1.1";
            versionToolMock.Setup(m => m.currentVersionAsync(song)).Returns(Task.FromResult(initialVersion));

            string versionNumber = await versionToolMock.Object.newVersionNumberAsync(song, compo, mix, mastering);

            Assert.Equal(expectedVersionNumber, versionNumber);
        }
    }
}
