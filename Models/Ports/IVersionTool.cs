using System.Threading.Tasks;

namespace App1.Models.Ports
{
    public interface IVersionTool
    {
        public Task uploadSongAsync(Song song, string title, string description);
        public Task updateSongAsync(Song song);
        public Task revertSongAsync(Song song);
    }
}