using System.Runtime.Serialization;

namespace WinUIApp
{
    public class FileManagerException : Exception
    {
        public FileManagerException(string? message) : base(message)
        {
        }
    }
}