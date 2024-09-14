using System;
using System.Windows;
using Backend.Utilities;  

namespace GUI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LogLevel logLevel = LogLevel.Info;

            if (e.Args.Length > 0)
            {
                foreach (var arg in e.Args)
                {
                    if (arg.StartsWith("--logLevel="))
                    {
                        var logLevelArg = arg.Split('=')[1];
                        if (Enum.TryParse(logLevelArg, true, out LogLevel parsedLogLevel))
                        {
                            logLevel = parsedLogLevel;
                        }
                        else
                        {
                            Console.WriteLine($"Invalid log level: {logLevelArg}. Using default (Info).");
                        }
                    }
                }
            }

            Logger.SetLogLevel(logLevel);
            Console.WriteLine($"Log level set to: {logLevel}");
        }
    }
}
