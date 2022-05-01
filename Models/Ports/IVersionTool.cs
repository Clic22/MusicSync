﻿namespace App1.Models.Ports
{
    public interface IVersionTool
    {
        public Task uploadSongAsync(Song song, string title, string description, string versionNumber);
        public Task uploadSongAsync(Song song, string file, string title);
        public Task downloadSharedSongAsync(string songFolder, string sharedLink, string downloadLocalPath);
        public Task<string> shareSongAsync(Song song);
        public Task updateSongAsync(Song song);
        public Task<bool> updatesAvailableForSongAsync(Song song);
        public Task revertSongAsync(Song song);
        public Task<SongVersion> currentVersionAsync(Song song);
        public Task<List<SongVersion>> versionsAsync(Song song);
        public Task<List<SongVersion>> upcomingVersionsAsync(Song song);
        public async Task<string> newVersionNumberAsync(Song song, bool compo, bool mix, bool mastering)
        {
            SongVersion currentVersion = await currentVersionAsync(song);
 
            string versionNumber = currentVersion.Number;
            int compoNumber = 0;
            int mixNumber = 0;
            int masteringNumber = 0;
            if (versionNumber != string.Empty)
            {
                var numbers = versionNumber.Split('.').Select(int.Parse).ToList();
                compoNumber = numbers[0];
                mixNumber = numbers[1];
                masteringNumber = numbers[2];
                if (compo)
                {
                    compoNumber++;
                    mixNumber = 0;
                    masteringNumber = 0;
                }
                if (mix)
                {
                    mixNumber++;
                    masteringNumber = 0;
                }
                if (mastering)
                {
                    masteringNumber++;
                }
            }
            else
            {
                if (compo)
                {
                    compoNumber++;
                }
                if (mix)
                {
                    mixNumber++;
                }
                if (mastering)
                {
                    masteringNumber++;
                }
            }
            versionNumber = compoNumber + "." + mixNumber + "." + masteringNumber;
            return versionNumber;
        }
    }
}