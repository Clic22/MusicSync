using System.Threading.Tasks;

namespace App1.Models.Ports
{
    public interface IVersionTool
    {
        public Task<string> uploadSongAsync(Song song, string title, string description, string versionNumber);
        public Task<string> uploadSongAsync(Song song, string file, string title);
        public Task<string> updateSongAsync(Song song);
        public Task<string> revertSongAsync(Song song);
        public Task<string> songVersionDescriptionAsync(Song song);
        public Task<string> songVersionNumberAsync(Song song);
    }
}