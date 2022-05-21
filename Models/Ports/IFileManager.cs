namespace App1.Models.Ports
{
    public interface IFileManager
    {
        public void CreateFile(string file, string directoryPath);
        public bool FileExists(string file, string directoryPath);
        public void WriteFile(string content, string file, string directoryPath);
        public string ReadFile(string file, string directoryPath);
        public Task CopyFileAsync(string fileName, string sourceDir, string destinationDir);
        public void DeleteFile(string file, string directoryPath);
        public void RenameFolder(string formerFolderName, string newFolderName);
        public void RenameFile(string formerFileName, string newFileName, string directoryPath);
        public void CreateDirectory(ref string directoryPath);
        public bool DirectoryExists(string directoryPath);
        public void DeleteDirectory(string directoryPath);
        public void CopyDirectory(string sourceDir, string destinationDir);
        public void CopyDirectories(List<string> directoriesToCopied, string directorySrc, string directoryDst);
        public Task<string?> FindFileNameBasedOnExtensionAsync(string directoryPath, string extension);
        public void SyncFile(string srcPath, string dstPath, string file);
        public Task CompressDirectoryAsync(string DirectoryToBeCompressed, string ArchiveName, string ArchivePath);
        public Task UncompressArchiveAsync(string ArchiveToBeUncompressed, string destinationDir);
        public string FormatPath(string path);
    }
}
