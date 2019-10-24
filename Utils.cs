using System;
using System.IO;
using System.Diagnostics;

namespace Avantgarde.Lib
{
    public static class Utils
    {
        public const string LOG_FILE = "aglogs.log";

        /// <summary>
        /// Prettier logging method
        /// </summary>
        /// <param name="msg">The message</param>
        /// <param name="msgType">Message type or severity</param>
        /// <param name="logToFile">Log to file as well.</param>
        public static void Log(string msg, MsgType msgType = MsgType.info, bool logToFile = true)
        {
            ConsoleColor fgcolor = ConsoleColor.White;
            string theTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            if(msgType == MsgType.error)
            {
                fgcolor = ConsoleColor.Red;
            }
            else if(msgType == MsgType.warning)
            {
                fgcolor = ConsoleColor.Yellow;
            }
            if (logToFile)
            {
                File.AppendAllText(LOG_FILE, theTime + " [" + msgType.ToString() + "] :: " + msg + Environment.NewLine);
            }
                    
            Console.ForegroundColor = fgcolor;
            Console.WriteLine(theTime + " [" + msgType.ToString() + "] :: " + msg);            
            Console.ResetColor();
        }

        public static bool CloseProcess(string exeName, bool kill = false, bool waitAndKill = false)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(exeName);
                if(processes.Length == 0)
                {
                    Log($"{exeName}: No such process.", MsgType.warning);
                    return false;
                }
                foreach(Process proc in processes)
                {
                    if (kill)
                    {
                        Log($"Killing {exeName}...");
                        proc.Kill();
                        Log($"Killed {exeName}.");
                    }
                    else
                    {
                        Log($"Closing {exeName}...");
                        proc.CloseMainWindow();
                        proc.WaitForExit(5000);
                        
                        if (!proc.HasExited && waitAndKill)
                        {
                            Log($"{exeName} did not exit in time.", MsgType.warning);
                            Log($"Killing {exeName}...");
                            proc.Kill();
                            Log($"Killed {exeName}.");
                        }
                        if (proc.HasExited)
                        {
                            Log($"Closed {exeName}.");
                        }
                        else
                        {
                            Log($"Could not close {exeName}.", MsgType.warning);
                            return false;
                        }
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                Log(ex.ToString(), MsgType.error);
                return false;
            }
        }
     
        public enum MsgType { info, warning, error};
    }
}
