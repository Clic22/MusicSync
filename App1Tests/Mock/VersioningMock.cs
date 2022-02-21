using App1.Models;
using App1.Models.Ports;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App1Tests.Mock
{
    public class VersioningMock : IVersionTool
    {
        public VersioningMock() {
            SongsVersioned = new Dictionary<string, (Song, Song)>();
        }

        public async Task uploadSongAsync(Song song, string title, string description)
        {
            await Task.Run(() =>
            {
                
            });
        }

        public async Task updateSongAsync(Song song)
        {
            await Task.Run(() =>
            {
                
            });
        }

        public async Task revertSongAsync(Song song)
        {
            await Task.Run(() =>
            {
                
            });
        }

        public Dictionary<string, (Song, Song)> SongsVersioned;
    }
}
