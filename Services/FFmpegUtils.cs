using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MagillaStream.Services
{
    public static class FFmpegUtils
    {
        // Get a list of supported encoders from FFmpeg
        public static List<string> GetAvailableEncoders()
        {
            List<string> encoders = new List<string>();
            Process ffmpeg = new Process();

            // Command to get the list of FFmpeg encoders
            ffmpeg.StartInfo.FileName = "ffmpeg";
            ffmpeg.StartInfo.Arguments = "-encoders";
            ffmpeg.StartInfo.RedirectStandardOutput = true;
            ffmpeg.StartInfo.UseShellExecute = false;
            ffmpeg.StartInfo.CreateNoWindow = true;

            try
            {
                if (ffmpeg.Start())
                {
                    while (!ffmpeg.StandardOutput.EndOfStream)
                    {
                        string? line = ffmpeg.StandardOutput.ReadLine();

                        if (!string.IsNullOrEmpty(line) && line.Contains("Encoder"))
                        {
                            encoders.Add(line.Trim());
                        }
                    }

                    ffmpeg.WaitForExit();
                }
                else
                {
                    throw new Exception("FFmpeg process failed to start.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            // Detect the CPU and GPU vendors using system commands
            string cpuVendor = DetectCpuVendor();
            string gpuVendor = DetectGpuVendor();

            // Filter encoders based on detected CPU and GPU
            List<string> filteredEncoders = FilterEncodersByHardware(cpuVendor, gpuVendor, encoders);

            return filteredEncoders;
        }

        // Detects the GPU vendor based on the operating system
        private static string DetectGpuVendor()
        {
            try
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    return RunSystemCommand("wmic path win32_videocontroller get name");
                }
                else if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    if (IsMacOS())
                    {
                        // macOS: Use system_profiler to get GPU information
                        return RunSystemCommand("system_profiler SPDisplaysDataType | grep Chipset");
                    }
                    else
                    {
                        // Linux: Use lspci to list PCI devices and find the GPU
                        return RunSystemCommand("lspci | grep VGA");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting GPU: {ex.Message}");
            }

            return "Unknown";
        }

        // Detects the CPU vendor based on the operating system
        private static string DetectCpuVendor()
        {
            try
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    // Windows: Use environment variables
                    return Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "Unknown";
                }
                else if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    if (IsMacOS())
                    {
                        // macOS: Use sysctl to get CPU info
                        return RunSystemCommand("sysctl -n machdep.cpu.brand_string");
                    }
                    else
                    {
                        // Linux: Use lscpu to get CPU info
                        return RunSystemCommand("lscpu | grep Vendor");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting CPU: {ex.Message}");
            }

            return "Unknown";
        }

        // Helper function to check if the system is running macOS
        private static bool IsMacOS()
        {
            return Environment.OSVersion.Platform == PlatformID.MacOSX || (Environment.OSVersion.Platform == PlatformID.Unix && !System.IO.Directory.Exists("/proc"));
        }

        // Helper function to run system commands and get the result
        private static string RunSystemCommand(string command)
        {
            try
            {
                Process process = new Process();
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    // Use cmd.exe for Windows
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.Arguments = $"/c {command}";
                }
                else
                {
                    // Use zsh for macOS and bash for Linux
                    process.StartInfo.FileName = IsMacOS() ? "/bin/zsh" : "/bin/bash";
                    process.StartInfo.Arguments = $"-c \"{command}\"";
                }

                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output.Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running command {command}: {ex.Message}");
                return "Unknown";
            }
        }

        // Filters encoders by detected CPU and GPU vendors
        private static List<string> FilterEncodersByHardware(string cpuVendor, string gpuVendor, List<string> encoders)
        {
            List<string> filteredEncoders = new List<string>();

            // GPU-based filtering
            if (gpuVendor.Contains("NVIDIA"))
            {
                foreach (string encoder in encoders)
                {
                    if (encoder.Contains("nvenc"))  // NVIDIA NVENC encoders
                    {
                        filteredEncoders.Add(encoder);
                    }
                }
            }
            else if (gpuVendor.Contains("AMD") || gpuVendor.Contains("Advanced Micro Devices"))
            {
                foreach (string encoder in encoders)
                {
                    if (encoder.Contains("amf"))  // AMD AMF encoders
                    {
                        filteredEncoders.Add(encoder);
                    }
                }
            }
            else if (gpuVendor.Contains("Intel"))
            {
                foreach (string encoder in encoders)
                {
                    if (encoder.Contains("qsv"))  // Intel QSV encoders
                    {
                        filteredEncoders.Add(encoder);
                    }
                }
            }

            // CPU-based filtering (Intel/AMD/General-purpose)
            if (cpuVendor.Contains("Intel"))
            {
                foreach (string encoder in encoders)
                {
                    if (encoder.Contains("qsv"))  // Intel Quick Sync Video encoders
                    {
                        filteredEncoders.Add(encoder);
                    }
                }
            }
            else if (cpuVendor.Contains("AMD"))
            {
                foreach (string encoder in encoders)
                {
                    if (encoder.Contains("amf") || encoder.Contains("libx264") || encoder.Contains("libx265"))
                    {
                        filteredEncoders.Add(encoder);
                    }
                }
            }

            // Add general-purpose CPU encoders (H.264, H.265, AV1)
            foreach (string encoder in encoders)
            {
                if (encoder.Contains("libx264") || encoder.Contains("libx265") || encoder.Contains("libaom"))
                {
                    filteredEncoders.Add(encoder);  // Software-based encoders
                }
            }

            return filteredEncoders;
        }
    }
}
