using System.Threading.Tasks;

namespace App1.Models.Ports
{
    public interface IVersionTool
    {
        public Task<string> uploadSongAsync(Song song, string title, string description);
        public Task<string> updateSongAsync(Song song);
        public Task<string> revertSongAsync(Song song);
    }
}