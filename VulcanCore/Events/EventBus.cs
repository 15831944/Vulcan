using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Vulcan.Core.Events
{
    public class EventBus
    {
        private static EventBus Instance = new EventBus();

        private List<object> handlers = new List<object>();
        public void RegisterEventHandlers(object o)
        {
            this.handlers.Add(o);
        }
        public void UnregisterEventHandlers(object o)
        {
            handlers.Remove(o);
        }
        public void Dispatch(object e)
        {

            foreach (object o in handlers)
            {
                foreach (MethodInfo mi in o.GetType().GetMethods())
                {
                    ParameterInfo[] args = mi.GetParameters();
                    Type t = e.GetType();
                    if (args.Length == 1 && (t.IsSubclassOf(args[0].ParameterType) || args[0].ParameterType.Equals(t)))
                    {
                        foreach (Attribute a in EventSubscribe.GetCustomAttributes(mi))
                        {
                            if (a.GetType() == new EventSubscribe().GetType())
                                mi.Invoke(o, new object[] { e });
                        }
                    }
                }
            }

        }

        public static EventBus GetEventBus()
        {
            return Instance;
        }
    }
}
