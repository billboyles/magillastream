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
        private static readonly string LogFilePath = "FFmpegService.log";
        private static LogLevel CurrentLogLevel = LogLevel.Info;

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
                try
                {
                    File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Logging failed: {ex.Message}");
                }
            }
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
