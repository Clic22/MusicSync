using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace App1
{
    public interface IVersionTool
    {
        public Task uploadSongAsync(Song song, string title, string description);


        public Task updateSongAsync(Song song);


        public Task revertSongAsync(Song song);

    }
}