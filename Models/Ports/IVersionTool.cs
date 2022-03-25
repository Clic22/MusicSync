using System.Threading.Tasks;

namespace App1.Models.Ports
{
    public interface IVersionTool
    {
        public Task<string> uploadSongAsync(Song song, string title, string description, string versionNumber);
        public Task<string> uploadSongAsync(Song song, string file, string title);
        public Task<string> updateSongAsync(Song song);
        public Task<string> revertSongAsync(Song song);
        public Task<string> versionDescriptionAsync(Song song);
        public Task<string> versionNumberAsync(Song song);
        public Task<List<(string, string)>> versionsAsync(Song song);
        public async Task<string> newVersionNumberAsync(Song song, bool compo, bool mix, bool mastering)
        {
            string versionNumber = await versionNumberAsync(song);
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