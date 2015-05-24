using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vulcan.Core.Events
{
    public class EventCommandRecieved : AbstractEvent
    {
        public string Message
        {
            private set;
            get;
        }

        public EventCommandRecieved(string message)
        {
            this.Message = message;
        }
    }
}
