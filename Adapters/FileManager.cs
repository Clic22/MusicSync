using App1.Models.Ports;

namespace App1.Adapters
{
    public class FileManager : IFileManager
    {
        public async Task<string> findSongFile(string songLocalPath)
        {
            var folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(songLocalPath);
            var files = await folder.GetFilesAsync();
            foreach (var file in files)
            {
                if (file.Name.Contains(".song"))
                {
                    return file.Name;
                }
            }
            return string.Empty;
        }
    }
}
