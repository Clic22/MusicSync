using App1.Models.Ports;

namespace App1Tests.Mock
{
    public class FileManagerMock : IFileManager
    {
        public async Task<string> findSongFile(string songLocalPath)
        {
            return await Task.Run(() =>
            {
                return "file.song";
            });
        }
    }
}
