using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Backend.Utilities
{
    public class FFmpegUtils
    {
        private readonly string _ffmpegPath = Path.Combine(AppContext.BaseDirectory, "Backend", "ffmpeg", "ffmpeg.exe");

        // Map encoders to user-friendly names, containers, and presets
        private readonly Dictionary<string, (string, string, List<string>)> EncoderInfo = new()
        {
            { "libx264", ("H.264 (libx264)", "flv", new() { "ultrafast", "superfast", "veryfast", "faster", "fast", "medium", "slow", "slower", "veryslow" }) },
            { "h264_qsv", ("H.264 (Intel Quick Sync)", "flv", new() { "veryfast", "fast", "medium", "slow" }) },
            { "libx265", ("HEVC (libx265)", "mp4", new() { "ultrafast", "superfast", "veryfast", "faster", "fast", "medium", "slow", "slower", "veryslow" }) },
            { "hevc_qsv", ("HEVC (Intel Quick Sync)", "mp4", new() { "veryfast", "fast", "medium", "slow" }) },
            { "libaom_av1", ("AV1 (libaom-av1)", "mp4", new() { "realtime", "good", "best" }) },
            { "h264_nvenc", ("H.264 (NVIDIA NVENC)", "flv", new() { "p1", "p2", "p3", "p4", "p5", "p6", "p7" }) },
            { "hevc_nvenc", ("HEVC (NVIDIA NVENC)", "mp4", new() { "p1", "p2", "p3", "p4", "p5", "p6", "p7" }) },
            { "h264_amf", ("H.264 (AMD AMF)", "flv", new() { "balanced", "quality", "speed" }) },
            { "hevc_amf", ("HEVC (AMD AMF)", "mp4", new() { "balanced", "quality", "speed" }) }
        };

        public List<string> GetSupportedCodecs()
        {
            var supportedCodecs = new List<string>();

            try
            {
                Logger.LogDebug($"Checking if FFmpeg exists at {_ffmpegPath}");
                if (!File.Exists(_ffmpegPath))
                {
                    throw new FileNotFoundException($"FFmpeg executable not found at {_ffmpegPath}");
                }

                Logger.LogInfo("Running FFmpeg to get supported hardware accelerations");
                var hwaccels = string.Join(Environment.NewLine, RunProcess(_ffmpegPath, "-hwaccels")).ToLower();

                // Filter the encoder list based on hardware support
                foreach (var (encoder, (name, container, presets)) in EncoderInfo)
                {
                    if ((encoder.Contains("qsv") && hwaccels.Contains("qsv")) ||
                        (encoder.Contains("nvenc") && hwaccels.Contains("cuda")) ||
                        (encoder.Contains("amf") && hwaccels.Contains("amf")) ||
                        encoder.StartsWith("lib"))
                    {
                        supportedCodecs.Add(name); // Add user-friendly encoder name to the list
                        Logger.LogDebug($"Supported encoder found: {name}");
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Logger.LogError($"FileNotFoundError occurred: {e.Message}");
            }
            catch (Exception e)
            {
                Logger.LogError($"Error occurred while running FFmpeg: {e.Message}");
            }

            Logger.LogInfo("Supported encoders: " + string.Join(", ", supportedCodecs));
            return supportedCodecs;
        }

        private static List<string> RunProcess(string executable, string arguments)
        {
            Logger.LogDebug($"Running process: {executable} {arguments}");
            var processStartInfo = new ProcessStartInfo
            {
                FileName = executable,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process
            {
                StartInfo = processStartInfo
            };

            var output = new List<string>();

            try
            {
                process.Start();
                Logger.LogDebug("Process started successfully");

                while (!process.StandardOutput.EndOfStream)
                {
                    var line = process.StandardOutput.ReadLine();
                    if (line != null)
                    {
                        output.Add(line);
                        Logger.LogDebug($"Process output: {line}"); 
                    }
                }

                process.WaitForExit();
                Logger.LogInfo($"Process exited with code: {process.ExitCode}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error occurred while running process: {ex.Message}");
            }

            return output;
        }
    }
}
