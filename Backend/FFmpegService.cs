using System.Diagnostics;
using System;
using System.Collections.Generic;
using Backend.Utilities;

namespace Backend
{
    public class FFmpegService
    {
        private Process? _ffmpegProcess;
        private readonly string _ffmpegPath;
        private readonly FFmpegUtils _ffmpegUtils;

        public FFmpegService()
        {
            _ffmpegPath = Path.Combine(AppContext.BaseDirectory, "ffmpeg", "ffmpeg.exe");
            _ffmpegProcess = null;
            _ffmpegUtils = new FFmpegUtils();
        }

        public static string SanitizeUrl(string url)
        {
            url = url.Trim();
            Logger.LogDebug($"Sanitizing URL: {url}");
            if (url.EndsWith("/"))
                url = url.Substring(0, url.Length - 1);
            if (url.Contains("?"))
                url = url.Split('?')[0];
            if (url.StartsWith("http://") || url.StartsWith("https://") || url.StartsWith("rtmp://"))
                return url;
            Logger.LogWarning("Invalid URL format");
            return string.Empty;
        }

        public static string SanitizeStreamKey(string key)
        {
            Logger.LogDebug($"Sanitizing Stream Key: {key}");
            return key.Trim();
        }

        public static string SanitizeBitrate(string bitrate, string defaultBitrate = "6000k")
        {
            Logger.LogDebug($"Sanitizing Bitrate: {bitrate}");
            bitrate = bitrate.Trim().ToLower();
            if (bitrate.EndsWith("k") && int.TryParse(bitrate.TrimEnd('k'), out _))
            {
                Logger.LogDebug($"Valid bitrate with 'k': {bitrate}");
                return bitrate;
            }
            else if (int.TryParse(bitrate, out _))
            {
                Logger.LogDebug($"Plain numeric bitrate: {bitrate}");
                return bitrate + "k";
            }
            Logger.LogWarning($"Invalid bitrate format. Defaulting to: {defaultBitrate}");
            return defaultBitrate;
        }

        public static string SanitizeResolution(string resolution)
        {
            Logger.LogDebug($"Sanitizing Resolution: {resolution}");
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
                : "1920x1080";
        }

        public List<string> GetAvailableEncoders()
        {
            Logger.LogDebug("Fetching available encoders.");
            return _ffmpegUtils.GetSupportedCodecs();
        }

        public void StartStream(string obsStreamUrl, List<Tuple<string, string, bool, string, string, string>> outputServices, bool enablePTSGeneration)
        {
            if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
            {
                Logger.LogWarning("An existing FFmpeg process is running. Stopping it.");
                _ffmpegProcess.Kill();
                _ffmpegProcess.Dispose();
                _ffmpegProcess = null;
            }

            _ffmpegProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            var groupedServices = new Dictionary<string, List<Tuple<string, string>>>();

            foreach (var service in outputServices)
            {
                string url = SanitizeUrl(service.Item1);
                string streamKey = SanitizeStreamKey(service.Item2);
                bool reEncode = service.Item3;
                string bitrate = SanitizeBitrate(service.Item4);
                string resolution = SanitizeResolution(service.Item5);
                string encoder = reEncode ? service.Item6 : "copy";

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
                string bufsize = $"{int.Parse(bitrate.TrimEnd('k')) * 3}k";

                string encoder = reEncode ? settingsParts[3] : "copy";

                if (reEncode)
                {
                    arguments += $"-c:v {encoder} -b:v {bitrate} -maxrate {bitrate} -bufsize {bufsize} -s {resolution} -c:a aac -vsync cfr ";
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

                Logger.LogInfo($"Starting FFmpeg process with arguments: {arguments}");

                _ffmpegProcess.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        Logger.LogDebug(args.Data);
                    }
                };

                _ffmpegProcess.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        Logger.LogError(args.Data);
                    }
                };

                _ffmpegProcess.Start();
                _ffmpegProcess.BeginOutputReadLine();
                _ffmpegProcess.BeginErrorReadLine();
            }
        }

        public void StopStream()
        {
            if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
            {
                Logger.LogInfo("Stopping FFmpeg process.");
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
}