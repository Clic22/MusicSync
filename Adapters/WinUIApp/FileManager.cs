using App1.Models.Ports;
using System.IO.Compression;
using System.Linq;
using Windows.Storage;

namespace WinUIApp
{
    public class FileManager : IFileManager
    {
        public async Task<string> findFileNameBasedOnExtensionAsync(string directoryPath, string extension)
        {
            try
            {
                var folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(directoryPath);
                var files = await folder.GetFilesAsync();
                string fileName = files.First(file => file.Name.Contains(extension)).Name;
                return fileName;
            }
            catch
            {
                return string.Empty;
            }
        }

        public void CopyDirectories(List<string> directoriesToCopied, string directorySrc, string directoryDst)
        {
            foreach(var directory in directoriesToCopied.Where(x => Directory.Exists(directorySrc + x)))
            {
              CopyDirectory(directorySrc + directory, directoryDst + directory);
            }
        }

        public void CopyDirectory(string sourceDir, string destinationDir)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir);
            }
        }

        public async Task CopyFileAsync(string fileName, string sourceDir, string destinationDir)
        {
            var folderSrc = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(sourceDir);
            var folderDst = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(destinationDir);
            var files = await folderSrc.GetFilesAsync();
            await files.First(x => x.Name.Equals(fileName)).CopyAsync(folderDst);
        }

        public async Task CompressDirectoryAsync(string DirectoryToBeCompressed, string ArchiveName, string ArchivePath)
        {
            await Task.Run(() =>
            {
                ZipFile.CreateFromDirectory(DirectoryToBeCompressed, ArchivePath + ArchiveName);
            });
        }

        public async Task UncompressArchiveAsync(string ArchiveToBeUncompressed, string destinationDir)
        {
            await Task.Run(() =>
            {
                ZipFile.ExtractToDirectory(ArchiveToBeUncompressed, destinationDir, true);
            });
        }

        public void CreateDirectory(ref string directoryPath)
        {
            directoryPath = FormatPath(directoryPath);
            Directory.CreateDirectory(directoryPath);
        }

        public void CreateFile(string file, string directoryPath)
        {
            directoryPath = FormatPath(directoryPath);
            File.Create(directoryPath + file).Close();
        }

        public string FormatPath(string path)
        {
            if (path.Last() != '\\')
            {
                path = path + '\\';
            }
            return path;
        }

        public bool DirectoryExists(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                return true;
            }
            return false;
        }

        public void SyncFile(string srcPath, string dstPath, string file)
        {
            if (File.Exists(srcPath + file) && !File.Exists(dstPath + file))
            {
                File.Copy(srcPath + file, dstPath + file);
            }
            else if (!File.Exists(srcPath + file) && File.Exists(dstPath + file))
            {
                File.Delete(dstPath + file);
            }
        }
    }
}
