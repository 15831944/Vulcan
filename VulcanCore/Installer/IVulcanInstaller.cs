using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vulcan.Core.Installer
{
    public interface IVulcanInstaller
    {
        bool IsInstalled();
        bool ShouldInstall(VulcanConfiguration config);
        void Install();
        void Uninstall();
    }
}
