using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace SteamLanSync
{
    static class Program
    {
        private static TextWriterTraceListener logger;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Trace.AutoFlush = true;

            FileStream fs = File.Open("log.txt", FileMode.Create, FileAccess.Write, FileShare.Read);
            logger = new TextWriterTraceListener(fs);
            Debug.Listeners.Add(logger);
            
            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            Application.ApplicationExit += Application_ApplicationExit;

            
        }

        static void Application_ApplicationExit(object sender, EventArgs e)
        {
            logger.Flush();
            logger.Close();
            logger.Dispose();
        }


    }
}
