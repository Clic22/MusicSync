using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1
{
    internal class SongsStorage
    {
        public SongsStorage()
        {
            songs = new List<Song>();
        }

        public List<Song> songs { get; set; }

    }
}
