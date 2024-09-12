using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public static class Logger
{
    private static readonly string LogFilePath = "ffmpeg_service.log";

    // Logs a message to both the console and a log file
    public static void Log(string message)
    {
        // Log to console
        Console.WriteLine(message);

        // Log to file
        try
        {
            File.AppendAllText(LogFilePath, $"{DateTime.Now}: {message}{Environment.NewLine}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Logging failed: {ex.Message}");
        }
    }

    // Logs an error message
    public static void LogError(string message)
    {
        Log($"ERROR: {message}");
    }
}

public class FFmpegService
{
    private Process? _ffmpegProcess;

    public FFmpegService()
    {
        _ffmpegProcess = null;
    }

    // Sanitizes URLs to ensure proper format
    public static string SanitizeUrl(string url)
    {
        url = url.Trim();
        if (url.EndsWith("/"))
            url = url.Substring(0, url.Length - 1);
        if (url.Contains("?"))
            url = url.Split('?')[0];
        if (url.StartsWith("http://") || url.StartsWith("https://") || url.StartsWith("rtmp://"))
            return url;
        return string.Empty;
    }

    // Sanitizes stream keys to remove unwanted characters
    public static string SanitizeStreamKey(string key)
    {
        return key.Trim();
    }

    // Sanitizes bitrate value
    public static string SanitizeBitrate(string bitrate, string defaultBitrate = "6000k")
    {
        // Check if the input is a valid number with or without 'k' at the end
        bitrate = bitrate.Trim().ToLower();
        if (bitrate.EndsWith("k") && int.TryParse(bitrate.TrimEnd('k'), out _))
        {
            return bitrate;
        }
        else if (int.TryParse(bitrate, out _))  // Accept plain numbers without 'k'
        {
            return bitrate + "k";  // Append 'k' for consistency
        }
        return defaultBitrate;  // Return default bitrate if input is invalid
    }

    // Maps resolution values
    public static string SanitizeResolution(string resolution)
    {
        var resolutionMap = new Dictionary<string, string>()
        {
            { "360p", "640x360" },
            { "480p", "854x480" },
            { "720p", "1280x720" },
            { "1080p", "1920x1080" },
            { "1440p", "2560x1440" },
            { "4K", "3840x2160" }
        };

        return resolutionMap.TryGetValue(resolution, out string? sanitizedResolution) 
            ? sanitizedResolution 
            : "1920x1080";  // Default to 1080p
    }

    // Starts FFmpeg with multiple output services
    public void StartStream(string obsStreamUrl, List<Tuple<string, string, bool, string, string>> outputServices, bool enablePTSGeneration)
    {
        if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
        {
            Logger.Log("Stopping existing FFmpeg process.");
            _ffmpegProcess.Kill();
            _ffmpegProcess.Dispose();
            _ffmpegProcess = null;
        }

        _ffmpegProcess = new Process();
        _ffmpegProcess.StartInfo.FileName = "ffmpeg";

        var groupedServices = new Dictionary<string, List<Tuple<string, string>>>();

        foreach (var service in outputServices)
        {
            string url = SanitizeUrl(service.Item1);
            string streamKey = SanitizeStreamKey(service.Item2);
            bool reEncode = service.Item3;
            string bitrate = SanitizeBitrate(service.Item4);  // Add logging for sanitized bitrate
            string resolution = SanitizeResolution(service.Item5);

            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(streamKey))
            {
                string settingsKey = $"{resolution}_{bitrate}_{reEncode}";

                if (!groupedServices.TryGetValue(settingsKey, out var serviceList))
                {
                    serviceList = new List<Tuple<string, string>>();
                    groupedServices[settingsKey] = serviceList;
                }

                serviceList.Add(Tuple.Create(url, streamKey));
            }
            else
            {
                Logger.LogError("Invalid URL or Stream Key.");
            }
        }

        foreach (var settingsGroup in groupedServices)
        {
            string arguments = $"-i {obsStreamUrl} ";
            if (enablePTSGeneration)
            {
                arguments += "-fflags +genpts ";
            }

            string[] settingsParts = settingsGroup.Key.Split('_');
            string resolution = settingsParts[0];
            string bitrate = settingsParts[1];
            bool reEncode = bool.Parse(settingsParts[2]);
            string bufsize = $"{int.Parse(bitrate.TrimEnd('k')) * 3}k";  // Dynamic bufsize calculation

            if (reEncode)
            {
                arguments += $"-c:v libx264 -b:v {bitrate} -maxrate {bitrate} -bufsize {bufsize} -s {resolution} -c:a aac -vsync cfr ";
            }
            else
            {
                arguments += "-c copy ";
            }

            foreach (var service in settingsGroup.Value)
            {
                arguments += $"-f flv {service.Item1}/{service.Item2} ";
            }

            _ffmpegProcess.StartInfo.Arguments = arguments.Trim();
            _ffmpegProcess.StartInfo.RedirectStandardOutput = true;
            _ffmpegProcess.StartInfo.RedirectStandardError = true;
            _ffmpegProcess.StartInfo.UseShellExecute = false;
            _ffmpegProcess.StartInfo.CreateNoWindow = true;

            // Log when FFmpeg process is started
            Logger.Log("Starting FFmpeg process with arguments: " + arguments);

            _ffmpegProcess.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null)  // Check if args.Data is not null
                {
                    Logger.Log(args.Data);
                }
            };

            _ffmpegProcess.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)  // Check if args.Data is not null
                {
                    Logger.LogError(args.Data);
                }
            };

            _ffmpegProcess.Start();
            _ffmpegProcess.BeginOutputReadLine();
            _ffmpegProcess.BeginErrorReadLine();
        }
    }

    // Stops the current FFmpeg process
    public void StopStream()
    {
        if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
        {
            Logger.Log("Stopping FFmpeg process.");
            _ffmpegProcess.Kill();
            _ffmpegProcess.Dispose();
            _ffmpegProcess = null;
        }
        else
        {
            Logger.LogError("Attempted to stop FFmpeg, but no process was running.");
        }
    }
}
