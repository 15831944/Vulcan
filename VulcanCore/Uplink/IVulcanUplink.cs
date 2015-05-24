using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vulcan.Core.PluginFramework;

namespace Vulcan.Core.Uplink
{
    public interface IVulcanUplink
    {
        void IdentifySelf();
        bool CheckForUpdate();
        bool DownloadUpdate();
        void UpdatePlugins(VulcanPluginManager config);
        void SendFile(string file);
        void SendFile(string file, string description);
        void SendMessage(string title, string message);
    }
}
