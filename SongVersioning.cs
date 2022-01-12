using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1
{
    internal class SongVersioning
    {
        public SongVersioning()
        {
            versionTool_ = new VersionTool();
        }

        public void updateLocalSongs()
        {
            string song = @"'C:\Users\Aymeric Meindre\Documents\Studio One\Songs\Collaboration\End of the Road'"; // directory of the git repository
            updateLocalSong(song);
        }

        public void pushNewSongsVersions()
        {
            string song = @"'C:\Users\Aymeric Meindre\Documents\Studio One\Songs\Collaboration\End of the Road'"; // directory of the git repository
            pushNewSongVersion(song);
        }

        private void updateLocalSong(string song)
        {
            versionTool_.pullChangesFromRepo(song);
        }

        private void pushNewSongVersion(string song)
        {
            versionTool_.addAllChanges(song);
            versionTool_.commitChanges(song);
            versionTool_.pushChangesToRepo(song);
        }

        private VersionTool versionTool_;
    }
}