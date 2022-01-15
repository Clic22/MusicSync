using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1
{
    public class Song
    {
        public Song()
        {
            title = string.Empty;
            localPath = string.Empty;
        }

        public Song(string newTitle, string newLocalPath)
        {
            title = newTitle;
            localPath = newLocalPath;
        }
        
        public string title { get; set; }
        public string localPath { get; set; }
    }
}
