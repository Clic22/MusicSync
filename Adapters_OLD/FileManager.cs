using App1.Models.Ports;
using System.Linq;

namespace App1.Adapters
{
    public class FileManager : IFileManager
    {
        public async Task<string> findSongFile(string songLocalPath)
        {
            try
            {
                var folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(songLocalPath);
                var files = await folder.GetFilesAsync();
                string songFile = string.Empty;
                songFile = files.Where(file => file.Name.Contains(".song")).First().Name;
                foreach (var file in files)
                {
                    if (file.Name.Contains(".song"))
                    {
                        songFile = file.Name;
                    }
                }
                return songFile;
            }
            catch
            {
                return string.Empty;
            }

        }
    }
}
