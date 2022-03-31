using App1.Models.Ports;

namespace App1Tests.Mock
{
    public class FileManagerMock : IFileManager
    {
        public async Task<string> findFileNameBasedOnExtensionAsync(string directory, string extension)
        {
            return await Task.Run(() =>
            {
                return "file.song";
            });
        }

        public void CopyDirectory(string directorySrc, string directoryDst, bool recursive)
        {

        }

        public Task CopyFileAsync(string fileName, string sourceDir, string destinationDir)
        {
            throw new NotImplementedException();
        }

        public Task CompressDirectoryAsync(string DirectoryToBeCompressed, string ArchiveName, string ArchivePath)
        {
            throw new NotImplementedException();
        }

        public Task UncompressArchiveAsync(string ArchiveToBeUncompressed, string destinationDir)
        {
            throw new NotImplementedException();
        }
    }
}
