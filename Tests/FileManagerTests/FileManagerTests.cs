using App1.Models.Ports;
using Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WinUIApp;

namespace WinUIAppTests.FileManagerTests
{

    public class FileManagerTests
    {
        [Theory]
        [InlineData(".song")]
        [InlineData(".zip")]
        public async Task findFileBaseOnExtensionTest(string extensionRequested)
        {
            string extension = extensionRequested;
            string fileName = "file";
            string directoryPath = @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory\";

            Directory.CreateDirectory(directoryPath);
            File.CreateText(directoryPath + fileName + extension).Close();
            Assert.True(File.Exists(directoryPath + fileName + extension));

            string expectedFileName = fileName + extension;

            IFileManager fileManager = new FileManager();
            fileName = await fileManager.findFileNameBasedOnExtensionAsync(directoryPath, extension);

            Assert.Equal(expectedFileName, fileName);

            Directory.Delete(directoryPath, true);
        }

        [Fact]
        public void copyDirectoriesTest()
        {
            string directoryPath = @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory\";
            string directorySrc = directoryPath + @"Src\";
            string directoryDst = directoryPath + @"Dst\";
            string songFile = "file.song";
            string zipFile = "file.zip";

            Directory.CreateDirectory(directorySrc);
            Directory.CreateDirectory(directoryDst);
            File.CreateText(directorySrc + songFile).Close();
            File.CreateText(directorySrc + zipFile).Close();

            Assert.True(File.Exists(directorySrc + songFile));
            Assert.True(File.Exists(directorySrc + zipFile));

            IFileManager fileManager = new FileManager();
            fileManager.CopyDirectory(directorySrc, directoryDst, false);

            Assert.True(File.Exists(directoryDst + songFile));
            Assert.True(File.Exists(directoryDst + zipFile));

            Directory.Delete(directoryPath, true);
        }

        [Fact]
        public void copySubDirectoriesTest()
        {
            string directoryPath = @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory\";
            string directorySrc = directoryPath + @"Src\";
            string directorySrcSub = directorySrc + @"Sub\";
            string directoryDst = directoryPath + @"Dst\";
            string directoryDstSub = directoryDst + @"Sub\";
            string songFile = "file.song";
            string zipFile = "file.zip";

            Directory.CreateDirectory(directorySrcSub);
            Directory.CreateDirectory(directoryDstSub);
            File.CreateText(directorySrc + songFile).Close();
            File.CreateText(directorySrcSub + zipFile).Close();
            Assert.True(File.Exists(directorySrc + songFile));
            Assert.True(File.Exists(directorySrcSub + zipFile));

            IFileManager fileManager = new FileManager();
            fileManager.CopyDirectory(directorySrc, directoryDst, true);

            Assert.True(File.Exists(directoryDst + songFile));
            Assert.True(File.Exists(directoryDstSub + zipFile));

            Directory.Delete(directoryPath, true);
        }

        [Theory]
        [InlineData("file.song")]
        [InlineData("file.zip")]
        public async Task copyFileTest(string requestedFileToCopy)
        {
            string directoryPath = @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory\";
            string directorySrc = directoryPath + @"Src\";
            string directoryDst = directoryPath + @"Dst\";
            string fileToCopy = requestedFileToCopy;

            Directory.CreateDirectory(directorySrc);
            Directory.CreateDirectory(directoryDst);
            File.CreateText(directorySrc + fileToCopy).Close();

            Assert.True(File.Exists(directorySrc + fileToCopy));

            IFileManager fileManager = new FileManager();
            await fileManager.CopyFileAsync(fileToCopy, directorySrc, directoryDst);

            Assert.True(File.Exists(directoryDst + fileToCopy));

            Directory.Delete(directoryPath, true);
        }

        [Theory]
        [InlineData("testCompressDirectory")]
        [InlineData("End of the Road")]
        public async Task compressDirectoryTest(string requestedDirectoryToCompress)
        {
            string directoryPath = @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory\";
            string directoryToBeCompressed = directoryPath + requestedDirectoryToCompress + @"\";
            string directoryDst = directoryPath + @"Dst\";
            string fileInDirectoryToBecompressed = "file.song";

            Directory.CreateDirectory(directoryToBeCompressed);
            Directory.CreateDirectory(directoryDst);
            File.CreateText(directoryToBeCompressed + fileInDirectoryToBecompressed).Close();

            Assert.True(File.Exists(directoryToBeCompressed + fileInDirectoryToBecompressed));

            IFileManager fileManager = new FileManager();
            string ArchiveName = requestedDirectoryToCompress + ".zip";
            await fileManager.CompressDirectoryAsync(directoryToBeCompressed, ArchiveName, directoryDst);

            Assert.True(File.Exists(directoryDst + ArchiveName));

            Directory.Delete(directoryPath, true);
        }

        [Theory]
        [InlineData("testCompressDirectory")]
        [InlineData("End of the Road")]
        public async Task uncompressDirectoryTest(string requestedDirectoryToCompress)
        {
            string directoryPath = @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory\";
            string directoryToBeCompressed = directoryPath + requestedDirectoryToCompress + @"\";
            string directorySrc = directoryPath + @"Src\";
            string directoryDst = directoryPath + @"Dst\";
            string fileInDirectoryToBeUncompressed = "file.song";

            Directory.CreateDirectory(directoryToBeCompressed);
            Directory.CreateDirectory(directorySrc);
            File.CreateText(directoryToBeCompressed + fileInDirectoryToBeUncompressed).Close();

            IFileManager fileManager = new FileManager();
            string ArchiveName = requestedDirectoryToCompress + ".zip";
            await fileManager.CompressDirectoryAsync(directoryToBeCompressed, ArchiveName, directorySrc);

            string ArchiveToBeUncompressed = directorySrc + ArchiveName;
            await fileManager.UncompressArchiveAsync(ArchiveToBeUncompressed, directoryDst);

            Assert.True(File.Exists(directoryDst + fileInDirectoryToBeUncompressed));

            Directory.Delete(directoryPath, true);
        }
    }
}