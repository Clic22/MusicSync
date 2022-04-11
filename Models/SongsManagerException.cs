using System.Runtime.Serialization;

namespace App1.Models
{
    [Serializable]
    public class SongsManagerException : Exception
    {
        public SongsManagerException(string? message) : base(message)
        {
        }
    }
}