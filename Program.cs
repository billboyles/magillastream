using Avalonia;
using Avalonia.ReactiveUI;
using System;

namespace MagillaStream
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                LogToFile($"Unhandled exception: {ex.Message}");
                LogToFile($"Stack Trace: {ex.StackTrace}");
                throw; // Re-throw the exception after logging it
            }
        }

        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<Views.App>()
                             .UsePlatformDetect()
                             .LogToTrace()
                             .UseReactiveUI();
        }

        private static void LogToFile(string message)
        {
            var logFilePath = "Logs/AppLoaderError.log";
            var logMessage = $"{DateTime.Now}: {message}\n";
            System.IO.File.AppendAllText(logFilePath, logMessage);
        }
    }
}
