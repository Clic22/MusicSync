using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1.Models.Ports
{
    public interface IFileManager
    {
        public Task<string> findSongFile(string songLocalPath, string songFile);
    }
}
