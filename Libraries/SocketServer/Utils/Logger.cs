using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;

namespace SocketServer.Utils
{
    public static class Logger
    {
        private static object _fileLock = new object();
        private static string _logFile = null;

        public static void Initialize(string logFile)
        {
            _logFile = logFile;
        }

        public static void Info(string format, params object[] args)
        {
            Log("INFO", format, args);
        }

        public static void Error(string format, params object[] args)
        {
            Log("ERROR", format, args);
        }

        public static void Log(string level, string format, params object[] args)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(level);
            sb.Append(":");
            sb.AppendFormat(format, args);
            sb.AppendLine();

            string message = sb.ToString();

            Console.Write(message);

            lock (_fileLock)
            {
                if (!string.IsNullOrEmpty(_logFile))
                {
                    File.AppendAllText(_logFile, message);
                }
            }
        }
    }
}
