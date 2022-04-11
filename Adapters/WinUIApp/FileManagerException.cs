using System.Runtime.Serialization;

namespace WinUIApp
{
    [Serializable]
    public class FileManagerException : Exception
    {
        public FileManagerException(string? message) : base(message)
        {
        }
    }
}