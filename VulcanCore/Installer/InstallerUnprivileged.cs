using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vulcan.Core.Installer
{
    public class InstallerUnprivileged : IVulcanInstaller
    {
        public bool IsInstalled()
        {
            return false;
        }

        public bool ShouldInstall(VulcanConfiguration config)
        {
            return config.RunLevel == VulcanRunLevel.User;
        }

        public void Install()
        {

        }

        public void Uninstall()
        {

        }
    }
}
