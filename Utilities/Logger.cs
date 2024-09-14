
using System.IO;

public static class Logger
{
    private static readonly string logFilePath = "Logs/app_log.txt";

    public static void Log(string message)
    {
        File.AppendAllText(logFilePath, message + "\n");
    }
}
