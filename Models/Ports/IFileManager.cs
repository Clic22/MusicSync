namespace App1.Models.Ports
{
    public interface IFileManager
    {
        public Task<string> findSongFile(string songLocalPath);
    }
}
