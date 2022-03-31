using App1.Models.Ports;
using System.IO.Compression;
using System.Linq;
using Windows.Storage;

namespace WinUIApp
{
    public class FileManager : IFileManager
    {
        public async Task<string> findFileNameBasedOnExtensionAsync(string directoryPath, string fileExtension)
        {
            try
            {
                var folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(directoryPath);
                var files = await folder.GetFilesAsync();
                string fileName = files.Where(file => file.Name.Contains(fileExtension)).First().Name;
                return fileName;
            }
            catch
            {
                return string.Empty;
            }
        }

        public void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
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
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        public async Task CopyFileAsync(string fileName, string sourceDir, string destinationDir)
        {
            var folderSrc = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(sourceDir);
            var folderDst = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(destinationDir);
            var files = await folderSrc.GetFilesAsync();
            await files.Where(x => x.Name.Equals(fileName)).First().CopyAsync(folderDst);
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
                ZipFile.ExtractToDirectory(ArchiveToBeUncompressed, destinationDir);
            });
        }
    }
}
