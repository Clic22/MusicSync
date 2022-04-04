namespace App1.Models.Ports
{
    public interface IFileManager
    {
        public void CreateDirectory(ref string directoryPath);
        public void CreateFile(string file, string directoryPath);
        public Task<string> findFileNameBasedOnExtensionAsync(string directoryPath, string extension);
        public void CopyDirectory(string sourceDir, string destinationDir);
        public void CopyDirectories(List<string> directoriesToCopied, string directorySrc, string directoryDst);
        public Task CopyFileAsync(string fileName, string sourceDir, string destinationDir);
        public Task CompressDirectoryAsync(string DirectoryToBeCompressed, string ArchiveName, string ArchivePath);
        public Task UncompressArchiveAsync(string ArchiveToBeUncompressed, string destinationDir);
        public void FormatPath(ref string path);
    }
}
