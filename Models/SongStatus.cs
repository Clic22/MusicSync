using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1.Models
{
    public class SongStatus
    {
        public SongStatus()
        {
            whoLocked = string.Empty;
            state = State.upToDate;
        }

        public string whoLocked;
        public State state;

        public enum State
        {
            upToDate,
            locked
        }
    }
}
