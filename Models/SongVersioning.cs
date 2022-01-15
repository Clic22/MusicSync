using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1
{
    internal class SongVersioning
    {
        public SongVersioning(SongsStorage songsList)
        {
            versionTool_ = new VersionTool();

            songsList_ = songsList;
        }

        public void updateLocalSongs()
        {
            foreach (Song song in songsList_)
                updateLocalSong(song);
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
        private SongsStorage songsList_;
    }
}