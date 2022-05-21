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

        public string GetWorkspaceForSong(Song song)
        {
            string workspaceForSong = fileManager.FormatPath(musicSyncFolder + song.Guid);
            fileManager.CreateDirectory(ref workspaceForSong);
            return workspaceForSong;
        }

        public string GetWorkspace(string workspaceName)
        {
            string workspace = fileManager.FormatPath(musicSyncFolder + workspaceName);
            fileManager.CreateDirectory(ref workspace);
            return workspace;
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
