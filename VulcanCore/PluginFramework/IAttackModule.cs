using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vulcan.Core.PluginFramework
{
    public interface IAttackModule
    {
        void InitModule(VulcanConfiguration config);
    }
}
