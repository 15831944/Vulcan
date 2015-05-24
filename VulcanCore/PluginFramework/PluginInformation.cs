using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vulcan.Core.PluginFramework
{
    public class PluginInformation
    {
        public string Name
        {
            private set;
            get;
        }

        public string FilePath
        {
            private set;
            get;
        }

        public string Hash
        {
            private set;
            get;
        }

        public bool Enabled
        {
            private set;
            get;
        }

        public bool Persistent
        {
            private set;
            get;
        }

        internal PluginInformation(string name, string filepath, string hash, bool enable, bool persistent)
        {
            this.Name = name;
            this.FilePath = filepath;
            this.Hash = hash;
            this.Enabled = enable;
            this.Persistent = persistent;
        }
    }
}
