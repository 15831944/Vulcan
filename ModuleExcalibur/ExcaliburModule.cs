using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vulcan.Core;
using Vulcan.Core.PluginFramework;

namespace ModuleExcalibur
{
    public class ExcaliburModule : IAttackModule
    {
        private ExcaliburThread excaliburThread = new ExcaliburThread();

        public void InitModule(VulcanConfiguration config)
        {
            if (config.RunLevel == VulcanRunLevel.System)
            {
                excaliburThread.Start();
            }
        }
    }
}
