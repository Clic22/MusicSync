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

            songsStorage_ = new SongsStorage();
            Song endOfTheRoad = new Song("End of the Road", 
                                         @"'C:\Users\Aymeric Meindre\Documents\Studio One\Songs\Collaboration\End of the Road'",
                                         @"'https://gitlab.com/instant-t-band/end-of-the-road.git'");
            songsStorage_.songs.Add(endOfTheRoad);
        }

        public void updateLocalSongs()
        {
            foreach (Song song in songsStorage_.songs)
                updateLocalSong(song);
        }

        public void pushNewSongsVersions(string changeTitle, string changeDescription)
        {
            foreach (Song song in songsStorage_.songs)
                pushNewSongVersion(song,changeTitle, changeDescription);
        }

        public void updateLocalSong(Song song)
        {
            versionTool_.pullChangesFromRepo(song.localPath);
        }

        public void pushNewSongVersion(Song song,string changeTitle,string changeDescription)
        {
            versionTool_.addAllChanges(song.localPath);
            versionTool_.commitChanges(song.localPath,changeTitle,changeDescription);
            versionTool_.pushChangesToRepo(song.localPath);
        }

        private VersionTool versionTool_;
        public SongsStorage songsStorage_ { get; set; }
    }
}