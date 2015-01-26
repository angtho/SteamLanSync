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

            // set up debug logger
            FileStream fs = File.Open("log.txt", FileMode.Create, FileAccess.Write, FileShare.Read);
            logger = new TextWriterTraceListener(fs);
            Debug.Listeners.Add(logger);

            // set up exception logging
            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            Application.ApplicationExit += Application_ApplicationExit;

            
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception)
                logException((Exception)e.ExceptionObject);
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            logException(e.Exception);
        }

        static void Application_ApplicationExit(object sender, EventArgs e)
        {
            logger.Flush();
            logger.Close();
            logger.Dispose();
        }

        private static void logException(Exception ex)
        {
            Debug.WriteLine("An unhandled exception occurred");
            Debug.WriteLine(ex.Message + "\n\nStack Trace:\n" + ex.StackTrace);
        }

    }
}
