using App1.Models.Ports;
using System.IO.Compression;

namespace WinUIApp
{
    public class FileManager : IFileManager
    {
        public async Task<string?> findFileNameBasedOnExtensionAsync(string directoryPath, string extension)
        {
            var folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(directoryPath);
            var files = await folder.GetFilesAsync();
            string? fileName = string.Empty;
            try
            {
                fileName = files.First(file => file.Name.Contains(extension)).Name;
            }
            catch
            {
                fileName = null;
            }
            
            return fileName;
        }

        public void CopyDirectories(List<string> directoriesToCopied, string directorySrc, string directoryDst)
        {
            foreach (var directory in directoriesToCopied.Where(x => Directory.Exists(directorySrc + x)))
            {
                CopyDirectory(directorySrc + directory, directoryDst + directory);
            }
        }

        public void CopyDirectory(string sourceDir, string destinationDir)
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

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

        public void WriteFile(string content, string file, string directoryPath)
        {
            File.WriteAllText(directoryPath + file, content);
        }

        public string ReadFile(string file, string directoryPath)
        {
            return File.ReadAllText(directoryPath + file);
        }



        public void DeleteDirectory(string directoryPath)
        {
            var directory = new DirectoryInfo(directoryPath) { Attributes = System.IO.FileAttributes.Normal };

            foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
            {
                info.Attributes = System.IO.FileAttributes.Normal;
            }

            directory.Delete(true);
        }

        public void DeleteFile(string file, string directoryPath)
        {
            File.Delete(directoryPath + file);
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

        public bool FileExists(string file, string directoryPath)
        {
            if (File.Exists(directoryPath + file))
            {
                return true;
            }
            return false;
        }

        public void RenameFolder(string formerFolderName, string newFolderName)
        {
            Directory.Move(formerFolderName, newFolderName);

        }

        public void RenameFile(string formerFileName, string newFileName, string directoryPath)
        {
            File.Move(directoryPath + formerFileName, directoryPath + newFileName);
        }
    }
}
