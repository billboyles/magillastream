using System.IO;
using System;

namespace MagillaStream.Utilities
{
    public static class Logger
    {
        private static readonly string logFilePath = "Logs/AppLog.log";

        public static void Log(string level, string message)
        {
            // Create a log message with the current date and time
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {level}: {message}";

            // Append the log message to the log file
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        }
        
        public static void Debug(string message)
        {
            Log("DEBUG", message);
        } 
        public static void Info(string message)
        {
            Log("INFO", message);
        }

        public static void Warning(string message)
        {
            Log("WARNING", message);
        }

        public static void Error(string message)
        {
            Log("ERROR", message);
        }
    }
}