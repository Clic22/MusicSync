using App1.Models.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1.Models
{
    public class MusicSyncWorkspace
    {
        public MusicSyncWorkspace(ISaver saver, IFileManager fileManager)
        {
            this.saver = saver;
            this.fileManager = fileManager; 
        }

        public string workspaceForSong(Song song)
        {
            string workspaceForSong = fileManager.FormatPath(musicSyncFolder + song.Guid);
            fileManager.CreateDirectory(ref workspaceForSong);
            return workspaceForSong;
        }

        public string musicSyncPathFromSharedLink(string sharedLink)
        {
            string guid = guidFromSharedLink(sharedLink);
            return fileManager.FormatPath(musicSyncFolder + guid);
        }

        public string guidFromSharedLink(string sharedLink)
        {
            User user = saver.savedUser();
            string UrlStart = "https://gitlab.com/" + user.BandName.Replace(" ", "-") + "/";
            string UrlEnd = ".git";
            int startPos = sharedLink.LastIndexOf(UrlStart) + UrlStart.Length;
            int length = sharedLink.IndexOf(UrlEnd) - startPos;
            string guid = sharedLink.Substring(startPos, length);
            return guid;
        }

        public string musicSyncFolder
        {
            get
            {
                return saver.savedMusicSyncFolder() + @".musicsync\";
            }
            private set
            {
                throw new InvalidOperationException();
            }
        }
        private readonly IFileManager fileManager;
        private readonly ISaver saver;
    }
}
