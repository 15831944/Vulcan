using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;

namespace Vulcan.Core
{
    public enum VulcanRunLevel
    {
        System = 0,
        User = 1,
    }

    public static class Privileges
    {
        private static bool isAdmin()
        {
            try
            {
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception)
            {
            }

            return false;
        }

        public static VulcanRunLevel GetPrivileges()
        {
            if (isAdmin())
                return VulcanRunLevel.System;
            else
                return VulcanRunLevel.User;
        }
    }
}
