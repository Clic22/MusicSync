namespace App1.Models.Ports
{
    public interface IFileManager
    {
        public Task<string> findFileNameBasedOnExtensionAsync(string directoryPath, string extension);
        public void CopyDirectory(string sourceDir, string destinationDir, bool recursive);
        public Task CopyFileAsync(string fileName, string sourceDir, string destinationDir);
        public Task CompressDirectoryAsync(string DirectoryToBeCompressed, string ArchiveName, string ArchivePath);
        public Task UncompressArchiveAsync(string ArchiveToBeUncompressed, string destinationDir);
    }
}
