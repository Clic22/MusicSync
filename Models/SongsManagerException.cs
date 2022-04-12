using System.Runtime.Serialization;

namespace App1.Models
{
    [Serializable]
    internal class SongsManagerException : Exception
    {
        public SongsManagerException()
        {
        }

        public SongsManagerException(string? message) : base(message)
        {
        }

        public SongsManagerException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected SongsManagerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}