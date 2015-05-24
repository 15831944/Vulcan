using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vulcan.Core;
using Vulcan.Core.PluginFramework;

namespace ModuleWatchee
{
    public class ModuleWatchee : IAttackModule
    {
        private WatcheeThread watchee = new WatcheeThread();

        public void InitModule(VulcanConfiguration config)
        {
            watchee.Start();
        }
    }
}
