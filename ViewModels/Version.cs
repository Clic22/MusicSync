using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1.ViewModels
{
    public class Version : Bindable
    {
        public Version()
        {
            versionNumber_ = string.Empty;
            versionDescription_ = string.Empty;
        }

        private string versionNumber_;
        public string VersionNumber
        {
            get
            {
                return versionNumber_;
            }
            set
            {
                SetProperty(ref versionNumber_, value);
            }
        }

        private string versionDescription_;
        public string VersionDescription
        {
            get
            {
                return versionDescription_;
            }
            set
            {
                SetProperty(ref versionDescription_, value);
            }
        }
    }
}
