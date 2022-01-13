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
            powershell_.Commands.Clear();
        }

        public void commitChanges(string directory, string title, string description)
        {
            goToSong(directory);
            powershell_.AddScript($"git commit -m '{title}\n\n{description}'").Invoke();
            powershell_.Commands.Clear();
        }

        public void pushChangesToRepo(string directory)
        {
            goToSong(directory);
            powershell_.AddScript(@"git push").Invoke();
            powershell_.Commands.Clear();
        }

        public void pullChangesFromRepo(string directory)
        {
            goToSong(directory);
            powershell_.AddScript(@"git pull").Invoke();
            powershell_.Commands.Clear();
        }

        private void goToSong(string directory)
        {
            powershell_.AddScript($"cd '{directory}'").Invoke();
            powershell_.Commands.Clear();
        }

        private PowerShell powershell_;
    }
}