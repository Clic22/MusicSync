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
