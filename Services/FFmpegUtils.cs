using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using MagillaStream.Utilities;

namespace MagillaStream.Services
{
    public static class FFmpegUtils
    {
        // List of commonly used software encoders for streaming
        private static readonly HashSet<string> CommonSoftwareEncoders = new HashSet<string>
        {
            "libx264", "libx265", "libvpx", "libaom-av1", // Video encoders
            "aac", "libmp3lame", "opus"                   // Audio encoders
        };

        // List of hardware encoder prefixes to capture
        private static readonly HashSet<string> HardwareEncoderPrefixes = new HashSet<string>
        {
            "nvenc", "qsv", "amf", "vaapi", "mf"          // NVIDIA, Intel QSV, AMD, VAAPI, Media Foundation
        };

        // Get a list of supported encoders from FFmpeg
        public static List<string> GetAvailableEncoders()
        {
            List<string> encoders = new List<string>();
            Process ffmpeg = new Process();

            // Get the correct path to the FFmpeg binary
            string ffmpegPath = GetFFmpegPath();

            // Command to get the list of FFmpeg encoders
            ffmpeg.StartInfo.FileName = ffmpegPath;
            ffmpeg.StartInfo.Arguments = "-encoders";
            ffmpeg.StartInfo.RedirectStandardOutput = true;
            ffmpeg.StartInfo.UseShellExecute = false;
            ffmpeg.StartInfo.CreateNoWindow = true;

            try
            {
                if (ffmpeg.Start())
                {
                    Logger.Debug("Starting FFmpeg process to get available encoders.");

                    bool captureEncoders = false;

                    while (!ffmpeg.StandardOutput.EndOfStream)
                    {
                        string? line = ffmpeg.StandardOutput.ReadLine();

                        // FFmpeg outputs a table of encoders after "Encoders:" header
                        if (line.Contains("Encoders:"))
                        {
                            captureEncoders = true;
                            continue;
                        }

                        // Capture video and audio encoders from valid lines
                        if (captureEncoders && !string.IsNullOrWhiteSpace(line) &&
                            (line.StartsWith(" V") || line.StartsWith(" A")) && !line.Contains("="))
                        {
                            // Extract the encoder name (second column)
                            var encoderName = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1];

                            // Filter relevant software encoders or hardware encoders
                            if (IsRelevantEncoder(encoderName))
                            {
                                encoders.Add(encoderName);
                                Logger.Debug($"Found encoder: {encoderName}");
                            }
                        }
                    }

                    ffmpeg.WaitForExit();
                    Logger.Debug("FFmpeg process completed.");
                }
                else
                {
                    throw new Exception("FFmpeg process failed to start.");
                }

                // Log the final list of encoders that will be added to the ComboBox
                Logger.Debug("Final list of encoders for ComboBox:");
                foreach (var encoder in encoders)
                {
                    Logger.Debug(encoder);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting FFmpeg encoders: {ex.Message}");
            }

            return encoders;
        }

        // Check if the encoder is relevant (either a common streaming software encoder or hardware encoder)
        private static bool IsRelevantEncoder(string encoderName)
        {
            // Check if it's a common software encoder
            if (CommonSoftwareEncoders.Contains(encoderName))
            {
                return true;
            }

            // Check if it's a hardware encoder based on the prefix
            foreach (var prefix in HardwareEncoderPrefixes)
            {
                if (encoderName.Contains(prefix))
                {
                    return true;
                }
            }

            return false;
        }

        // Get the correct FFmpeg path depending on the operating system
        private static string GetFFmpegPath()
        {
            string basePath = AppContext.BaseDirectory;
            string ffmpegFolder = Path.Combine(basePath, "FFmpeg");

            string ffmpegPath;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                ffmpegPath = Path.Combine(ffmpegFolder, "ffmpeg.exe");
            }
            else
            {
                ffmpegPath = Path.Combine(ffmpegFolder, "ffmpeg"); // For Unix/Mac
            }

            // Log the resolved path
            Logger.Debug($"Resolved FFmpeg Path: {ffmpegPath}");

            if (!File.Exists(ffmpegPath))
            {
                Logger.Error($"FFmpeg binary not found at: {ffmpegPath}");
                throw new Exception($"FFmpeg binary not found at: {ffmpegPath}");
            }

            return ffmpegPath;
        }
    }
}