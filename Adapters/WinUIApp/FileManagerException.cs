using System.Runtime.Serialization;

namespace WinUIApp
{
    public class FileManagerException : Exception
    {
        public FileManagerException()
        {
        }

        public FileManagerException(string? message) : base(message)
        {
        }

        public FileManagerException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected FileManagerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}