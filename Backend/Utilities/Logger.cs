using System;
using System.IO;

namespace Backend.Utilities
{
    public enum LogLevel
    {
        Info,
        Debug,
        Warning,
        Error
    }

    public static class Logger
    {
        private static readonly string LogDirectory = "Logs";
        private static LogLevel CurrentLogLevel = LogLevel.Info;

        static Logger()
        {
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
        }

        public static void SetLogLevel(LogLevel logLevel)
        {
            CurrentLogLevel = logLevel;
        }

        public static void Log(string message, LogLevel logLevel = LogLevel.Info)
        {
            if (logLevel >= CurrentLogLevel)
            {
                var logMessage = $"{DateTime.Now}: [{logLevel}] {message}";
                Console.WriteLine(logMessage);

                string logFilePath = GetLogFilePath(logLevel);
                try
                {
                    File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Logging failed: {ex.Message}");
                }
            }
        }

        private static string GetLogFilePath(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Error => Path.Combine(LogDirectory, "Error.log"),
                LogLevel.Warning => Path.Combine(LogDirectory, "Warning.log"),
                LogLevel.Debug => Path.Combine(LogDirectory, "Debug.log"),
                _ => Path.Combine(LogDirectory, "Info.log"),
            };
        }

        public static void LogError(string message)
        {
            Log(message, LogLevel.Error);
        }

        public static void LogWarning(string message)
        {
            Log(message, LogLevel.Warning);
        }

        public static void LogInfo(string message)
        {
            Log(message, LogLevel.Info);
        }

        public static void LogDebug(string message)
        {
            Log(message, LogLevel.Debug);
        }
    }
}
