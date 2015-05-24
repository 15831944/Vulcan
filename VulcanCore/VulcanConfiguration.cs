using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using Vulcan.Core.PluginFramework;
using Vulcan.Core.Uplink;

namespace Vulcan.Core
{
    public class VulcanConfiguration
    {

        public static VulcanConfiguration Instance;

        public string InstallLocation
        {
            private set;
            get;
        }

        public VulcanRunLevel RunLevel
        {
            private set;
            get;
        }

        public int Pid
        {
            private set;
            get;
        }

        public VulcanPluginManager PluginManager
        {
            private set;
            get;
        }

        public IVulcanUplink PrimaryUplink
        {
            private set;
            get;
        }

        internal VulcanConfiguration()
        {
            this.Pid = Process.GetCurrentProcess().Id;
            this.InstallLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            this.RunLevel = Privileges.GetPrivileges();
            this.PluginManager = new VulcanPluginManager();
            this.PrimaryUplink = new C2Uplink();

            Console.WriteLine("Current configuration:");
            Console.WriteLine("\tInstall Location: '{0}'", this.InstallLocation);
            Console.WriteLine("\tInstall Location: '{0}'", this.InstallLocation);
            Console.WriteLine("\tRun Level: '{0}'", this.RunLevel.ToString());
            Instance = this;
        }

    }
}
