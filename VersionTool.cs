using System.Management.Automation;

namespace App1
{
    internal class VersionTool
    {
        public VersionTool()
        {
            powershell_ = PowerShell.Create();
        }
        public void addAllChanges(string directory)
        {
            goToSong(directory);
            powershell_.AddScript(@"git add -A ").Invoke();
        }

        public void commitChanges(string directory)
        {
            goToSong(directory);
            powershell_.AddScript(@"git commit -m 'Automatic Commit from C# software'").Invoke();
        }

        public void pushChangesToRepo(string directory)
        {
            goToSong(directory);
            powershell_.AddScript(@"git push").Invoke();
        }

        public void pullChangesFromRepo(string directory)
        {
            goToSong(directory);
            powershell_.AddScript(@"git pull").Invoke();
        }

        private void goToSong(string directory)
        {
            powershell_.AddScript($"cd {directory}").Invoke();
        }

        private PowerShell powershell_;
    }
}