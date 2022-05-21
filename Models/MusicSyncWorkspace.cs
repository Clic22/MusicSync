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
            this._saver = saver;
            this._fileManager = fileManager; 
        }

        public string GetWorkspaceForSong(Song song)
        {
            string workspaceForSong = _fileManager.FormatPath(MusicSyncFolder + song.Guid);
            _fileManager.CreateDirectory(ref workspaceForSong);
            return workspaceForSong;
        }

        public string GetWorkspace(string workspaceName)
        {
            string workspace = _fileManager.FormatPath(MusicSyncFolder + workspaceName);
            _fileManager.CreateDirectory(ref workspace);
            return workspace;
        }

        public string MusicSyncFolder
        {
            get
            {
                return _saver.SavedMusicSyncFolder() + @".musicsync" + Path.DirectorySeparatorChar;
            }
            private set
            {
                throw new InvalidOperationException();
            }
        }
        private readonly IFileManager _fileManager;
        private readonly ISaver _saver;
    }
}
