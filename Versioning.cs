using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;

namespace App1
{
    internal class Versioning
    {
        public Versioning()
        {
            m_powershell = PowerShell.Create();
        }

        public void updateRepos()
        {
            string directory = @"'C:\Users\Aymeric Meindre\Documents\Studio One\Songs\Collaboration\End of the Road'"; // directory of the git repository
            goToDirectory(directory);
            pullRepo();
        }

        public void commitAllChanges()
        {
            string directory = @"'C:\Users\Aymeric Meindre\Documents\Studio One\Songs\Collaboration\End of the Road'"; // directory of the git repository
            goToDirectory(directory);
            addAll();
            commit();
            pushToRepo();

        }

        private void pullRepo()
        {
            m_powershell.AddScript(@"git pull").Invoke();
        }

        private void addAll()
        {
            m_powershell.AddScript(@"git add -A ").Invoke();
        }

        private void commit()
        {
            m_powershell.AddScript(@"git commit -m 'Automatic Commit from C# software'").Invoke();
        }

        private void pushToRepo()
        {
            m_powershell.AddScript(@"git push").Invoke();
        }

        private void goToDirectory(string directory)
        {
            m_powershell.AddScript($"cd {directory}").Invoke();
        }

        private PowerShell m_powershell;
    }
}
