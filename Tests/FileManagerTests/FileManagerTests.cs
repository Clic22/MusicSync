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
            string? fileName = "file";
            string directoryPath = @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory\";

            Directory.CreateDirectory(directoryPath);
            File.CreateText(directoryPath + fileName + extension).Close();
            Assert.True(File.Exists(directoryPath + fileName + extension));

            string expectedFileName = fileName + extension;

            IFileManager fileManager = new FileManager();
            fileName = await fileManager.FindFileNameBasedOnExtensionAsync(directoryPath, extension);

            Assert.Equal(expectedFileName, fileName);

            Directory.Delete(directoryPath, true);
        }

        [Fact]
        public void copyDirectoryTest()
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
            fileManager.CopyDirectory(directorySrc, directoryDst);

            Assert.True(File.Exists(directoryDst + songFile));
            Assert.True(File.Exists(directoryDst + zipFile));

            Directory.Delete(directoryPath, true);
        }

        [Fact]
        public void syncSrcFile()
        {
            string directoryPath = @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory\";
            string directorySrc = directoryPath + @"Src\";
            string directoryDst = directoryPath + @"Dst\";
            string songFile = "file.song";

            Directory.CreateDirectory(directorySrc);
            Directory.CreateDirectory(directoryDst);
            File.CreateText(directorySrc + songFile).Close();

            Assert.True(File.Exists(directorySrc + songFile));
            Assert.False(File.Exists(directoryDst + songFile));

            IFileManager fileManager = new FileManager();
            fileManager.SyncFile(directorySrc, directoryDst, songFile);

            Assert.True(File.Exists(directorySrc + songFile));
            Assert.True(File.Exists(directoryDst + songFile));

            Directory.Delete(directoryPath, true);
        }

        [Fact]
        public void syncDstFile()
        {
            string directoryPath = @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory\";
            string directorySrc = directoryPath + @"Src\";
            string directoryDst = directoryPath + @"Dst\";
            string songFile = "file.song";

            Directory.CreateDirectory(directorySrc);
            Directory.CreateDirectory(directoryDst);
            File.CreateText(directoryDst + songFile).Close();

            Assert.False(File.Exists(directorySrc + songFile));
            Assert.True(File.Exists(directoryDst + songFile));

            IFileManager fileManager = new FileManager();
            fileManager.SyncFile(directorySrc, directoryDst, songFile);

            Assert.False(File.Exists(directorySrc + songFile));
            Assert.False(File.Exists(directoryDst + songFile));

            Directory.Delete(directoryPath, true);
        }

        [Fact]
        public void copySubDirectoryTest()
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
            fileManager.CopyDirectory(directorySrc, directoryDst);

            Assert.True(File.Exists(directoryDst + songFile));
            Assert.True(File.Exists(directoryDstSub + zipFile));

            Directory.Delete(directoryPath, true);
        }

        [Fact]
        public void copyDirectoriesTest()
        {
            string directoryPath = @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory\";
            string directorySrc = directoryPath + @"Src\";
            string directoryDst = directoryPath + @"Dst\";
            string directorySong = @"Song\";
            string songFile = "file.song";
            string directoryMedia = @"Media\";
            string mediaFile = "file.wav";

            List<string> directoriesToCopied = new List<string>();
            directoriesToCopied.Add(directorySong);
            directoriesToCopied.Add(directoryMedia);

            Directory.CreateDirectory(directorySrc + directorySong);
            Directory.CreateDirectory(directorySrc + directoryMedia);
            Directory.CreateDirectory(directoryDst);
            File.CreateText(directorySrc + directorySong + songFile).Close();
            File.CreateText(directorySrc + directoryMedia + mediaFile).Close();

            Assert.True(File.Exists(directorySrc + directorySong + songFile));
            Assert.True(File.Exists(directorySrc + directoryMedia + mediaFile));

            IFileManager fileManager = new FileManager();
            fileManager.CopyDirectories(directoriesToCopied, directorySrc, directoryDst);

            Assert.True(Directory.Exists(directoryDst + directorySong));
            Assert.True(Directory.Exists(directoryDst + directoryMedia ));
            Assert.True(File.Exists(directoryDst + directorySong + songFile));
            Assert.True(File.Exists(directoryDst + directoryMedia + mediaFile));

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

        [Theory]
        [InlineData(@"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory")]
        public void createDirectoryTest(string requestedDirectoryToCreate)
        {
            string savedRequestedDirectoryToCreate = requestedDirectoryToCreate;
            IFileManager fileManager = new FileManager();
            fileManager.CreateDirectory(ref requestedDirectoryToCreate);
            Assert.Equal(savedRequestedDirectoryToCreate + '\\', requestedDirectoryToCreate);
            Assert.True(Directory.Exists(requestedDirectoryToCreate));

            Directory.Delete(requestedDirectoryToCreate, true);
        }

        [Theory]
        [InlineData(@"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory\")]
        public void createDirectoryWithBackSlashTest(string requestedDirectoryToCreate)
        {
            IFileManager fileManager = new FileManager();
            fileManager.CreateDirectory(ref requestedDirectoryToCreate);
            Assert.True(Directory.Exists(requestedDirectoryToCreate));

            Directory.Delete(requestedDirectoryToCreate, true);
        }

        [Theory]
        [InlineData("file.song", @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory")]
        public void createFileTest(string fileToCreate, string requestedDirectory)
        {
            string savedRequestedDirectory = requestedDirectory;
            IFileManager fileManager = new FileManager();
            fileManager.CreateDirectory(ref requestedDirectory);
            fileManager.CreateFile(fileToCreate, requestedDirectory);
            Assert.True(File.Exists(savedRequestedDirectory + '\\' + fileToCreate));

            Directory.Delete(requestedDirectory, true);
        }

        [Theory]
        [InlineData("file.song", @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory\")]
        public void createFileWithBackSlashTest(string fileToCreate, string requestedDirectory)
        {
            IFileManager fileManager = new FileManager();
            fileManager.CreateDirectory(ref requestedDirectory);
            fileManager.CreateFile(fileToCreate, requestedDirectory);
            Assert.True(File.Exists(requestedDirectory + fileToCreate));

            Directory.Delete(requestedDirectory, true);
        }

        [Theory]
        [InlineData(@"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory")]
        public void FormatPathTest(string path)
        {
            var savedPath = path;
            FileManager fileManager = new FileManager();
            string newPath = fileManager.FormatPath(path);
            Assert.Equal(savedPath + Path.DirectorySeparatorChar, newPath);

        }

        [Theory]
        [InlineData(@"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory\")]
        public void FormatPathWithBackSlashTest(string path)
        {
            var savedPath = path;
            FileManager fileManager = new FileManager();
            path = fileManager.FormatPath(path);
            Assert.Equal(savedPath, path);
        }

        [Theory]
        [InlineData(@"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory\")]
        public void DeleteDirectoryTest(string path)
        {
            IFileManager fileManager = new FileManager();
            fileManager.CreateDirectory(ref path);
            string subFolder = path + @"Sub\";
            fileManager.CreateDirectory(ref subFolder);

            fileManager.DeleteDirectory(path);

            Assert.False(fileManager.DirectoryExists(subFolder));
            Assert.False(fileManager.DirectoryExists(path));
        }

        [Theory]
        [InlineData("file.song", @"C:\Users\Aymeric Meindre\source\repos\MusicSync\Tests\testDirectory")]
        public void DeleteFileTest(string fileToDelete, string inDirectory)
        {
            IFileManager fileManager = new FileManager();
            fileManager.CreateDirectory(ref inDirectory);
            fileManager.CreateFile(fileToDelete, inDirectory);

            fileManager.DeleteFile(fileToDelete,inDirectory);

            Assert.False(File.Exists(inDirectory + fileToDelete));
            Directory.Delete(inDirectory, true);
        }
    }
}