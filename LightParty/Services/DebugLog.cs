using LightParty.Connection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LightParty.Services
{
    /// <summary>
    /// This class contains methods that create and update the DebugLog file, which can be used to write debug information to a file. This can be handy while debugging without Visual Studio.
    /// BridgeInformation.useDebugLog must be set to true in order to use the file.
    /// </summary>
    class DebugLog
    {
        private static StorageFolder storageFolder = ApplicationData.Current.LocalCacheFolder; //Folder, in which the file is saved.
        private static string logFilePath = "";

        /// <summary>
        /// Creates the DebugLog file and configures Trace.
        /// </summary>
        /// <returns>The path of DebugLog file</returns>
        public static string CreateDebugLog()
        {
            if (logFilePath == "")
            {
                logFilePath = Path.Combine(storageFolder.Path, "DebugLog.txt");
                Stream logFile = File.Create(logFilePath);
                Trace.Listeners.Add(new TextWriterTraceListener(logFile));
                Trace.AutoFlush = true;
            }

            return logFilePath;
        }

        /// <summary>
        /// Writes a new line to the DebugLog file with the help of trace.
        /// </summary>
        /// <param name="content">The content of the new line</param>
        /// <param name="importance">(Optional) How important the new line is; 0 = log, 1 = warning, 2 = error</param>
        /// <param name="exeception">(Optional) An exeception whose content is added to the new line</param>
        public static void WriteToLog(string content, int importance = 0, Exception exeception = null)
        {
            if (!BridgeInformation.useDebugLog)
                return;

            string prefix = "";
            switch (importance)
            {
                case 0:
                    prefix = "LOG";
                    break;
                case 1:
                    prefix = "WARNING";
                    break;
                case 2:
                    prefix = "ERROR";
                    break;
            }

            string logMessage = DateTime.Now + " - " + prefix + ": " + content;

            if (exeception != null)
                logMessage += " - Message: " + exeception.Message + " - InnerException: " + exeception.InnerException + " - Source: " + exeception.Source + " - StackTrace: " + exeception.StackTrace + " - TargetSite: " + exeception.TargetSite;
            
            Trace.WriteLine(logMessage);
        }
    }
}
