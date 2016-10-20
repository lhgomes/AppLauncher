using System;
using System.IO;

namespace AppLauncher.Class
{
    public class Logging : IDisposable
    {
        private string logFile = string.Empty;
        private static readonly object locker = new object();

        public Logging()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Version version = assembly.GetName().Version;

            logFile = string.Format("{0}{1}_{2}{3}{4}.log", System.IO.Path.GetTempPath(), assembly.GetName().Name, DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);

            WriteToLog(string.Format("LOG START - V.{0}.{1}.{2}.{3} - Running on {4}", version.Major,
                version.Minor, version.Build, version.Revision, WinOperatingSystem.GetCurrentWindowsVersion()));
        }

        public void WriteToLog(string Message)
        {
            lock (locker)
            {
                string message = string.Format("{0} - {1}", DateTime.Now.ToString(), Message);

                try
                {
                    StreamWriter SW;
                    SW = File.AppendText(logFile);
                    SW.WriteLine(message);
                    SW.Close();
                }
                catch { }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                WriteToLog("LOG FINISIH");
            }
        }
    }
}