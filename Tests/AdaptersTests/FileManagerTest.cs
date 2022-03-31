using App1.Models;
using App1.Models.Ports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WinUIApp;

namespace WinUIAppTests.FileManagerTests
{

    [TestClass]
    public class FileManagerTests
    {
        [TestMethod]
        [DataRow(".song")]
        [DataRow(".zip")]
        public async Task findFileBaseOnExtensionTest(string extensionRequested)
        {
            string extension = extensionRequested;
            string fileName = "file";
            string directoryPath = @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory\";

            Directory.CreateDirectory(directoryPath);
            File.CreateText(directoryPath + fileName + extension).Close();
            Assert.IsTrue(File.Exists(directoryPath + fileName + extension));

            string expectedFileName = fileName + extension;

            IFileManager fileManager = new FileManager();
            fileName = await fileManager.findFileNameBasedOnExtensionAsync(directoryPath,extension);

            Assert.AreEqual(expectedFileName,fileName);

            Directory.Delete(directoryPath,true);
        }

        [TestMethod]
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

            Assert.IsTrue(File.Exists(directorySrc + songFile));
            Assert.IsTrue(File.Exists(directorySrc + zipFile));

            IFileManager fileManager = new FileManager();
            fileManager.CopyDirectory(directorySrc, directoryDst,false);

            Assert.IsTrue(File.Exists(directoryDst + songFile));
            Assert.IsTrue(File.Exists(directoryDst + zipFile));

            Directory.Delete(directoryPath, true);
        }

        [TestMethod]
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
            Assert.IsTrue(File.Exists(directorySrc + songFile));
            Assert.IsTrue(File.Exists(directorySrcSub + zipFile));

            IFileManager fileManager = new FileManager();
            fileManager.CopyDirectory(directorySrc, directoryDst, true);

            Assert.IsTrue(File.Exists(directoryDst + songFile));
            Assert.IsTrue(File.Exists(directoryDstSub + zipFile));

            Directory.Delete(directoryPath, true);
        }

        [TestMethod]
        [DataRow("file.song")]
        [DataRow("file.zip")]
        public async Task copyFileTest(string requestedFileToCopy)
        {
            string directoryPath = @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory\";
            string directorySrc = directoryPath + @"Src\";
            string directoryDst = directoryPath + @"Dst\";
            string fileToCopy = requestedFileToCopy;

            Directory.CreateDirectory(directorySrc);
            Directory.CreateDirectory(directoryDst);
            File.CreateText(directorySrc + fileToCopy).Close();

            Assert.IsTrue(File.Exists(directorySrc + fileToCopy));

            IFileManager fileManager = new FileManager();
            await fileManager.CopyFileAsync(fileToCopy, directorySrc, directoryDst);

            Assert.IsTrue(File.Exists(directoryDst + fileToCopy));

            Directory.Delete(directoryPath, true);
        }

        [TestMethod]
        [DataRow("testCompressDirectory")]
        [DataRow("End of the Road")]
        public async Task compressDirectoryTest(string requestedDirectoryToCompress)
        {
            string directoryPath = @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory\";
            string directoryToBeCompressed = directoryPath + requestedDirectoryToCompress + @"\";
            string directoryDst = directoryPath + @"Dst\";
            string fileInDirectoryToBecompressed = "file.song";

            Directory.CreateDirectory(directoryToBeCompressed);
            Directory.CreateDirectory(directoryDst);
            File.CreateText(directoryToBeCompressed + fileInDirectoryToBecompressed).Close();

            Assert.IsTrue(File.Exists(directoryToBeCompressed + fileInDirectoryToBecompressed));

            IFileManager fileManager = new FileManager();
            string ArchiveName = requestedDirectoryToCompress + ".zip";
            await fileManager.CompressDirectoryAsync(directoryToBeCompressed, ArchiveName, directoryDst);

            Assert.IsTrue(File.Exists(directoryDst + ArchiveName));

            Directory.Delete(directoryPath, true);
        }

        [TestMethod]
        [DataRow("testCompressDirectory")]
        [DataRow("End of the Road")]
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

            Assert.IsTrue(File.Exists(directoryDst + fileInDirectoryToBeUncompressed));

            Directory.Delete(directoryPath, true);
        }
    }
}