using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vulcan.Core.Events
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class EventSubscribe : Attribute
    {
    }
}
