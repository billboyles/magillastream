using System.Collections.Generic;
using System.Diagnostics;
using MagillaStream.Models;

namespace MagillaStream.Services
{
    public class FFmpegService
    {
        private Process? _ffmpegProcess;

        // Start a single FFmpeg process for all output groups
        public void StartFFmpegProcess(Profile profile)
        {
            // Build the combined FFmpeg command for all output groups
            string finalCommand = BuildCombinedCommand(profile);

            // Start the FFmpeg process
            _ffmpegProcess = StartProcess(finalCommand);
        }

        // Build the FFmpeg command for multiple output groups
        private string BuildCombinedCommand(Profile profile)
        {
            string command = $"-i {profile.IncomingUrl}";  // Input stream

            // Add PTS flag if set in the profile
            if (profile.GeneratePTS)
            {
                command += " -fflags +genpts";  // Adds PTS generation to the command
            }

            // Loop over each output group to create encoder and tee commands
            foreach (var group in profile.OutputGroups)
            {
                // Generate the encoder and tee command for this output group
                string groupCommand = CreateGroupCommand(group);

                // Append the group command to the main FFmpeg command
                command += $" {groupCommand}";
            }

            return command;
        }

        // Creates the encoder and tee command for each OutputGroup
        private string CreateGroupCommand(OutputGroup group)
        {
            string command = $"-map 0";  // Maps the input stream (index 0)

            if (group.StreamSettings != null)
            {
                // Apply encoding settings from StreamSettings
                var streamSettings = group.StreamSettings;
                string bufferSize = CalculateBufferSize(streamSettings);

                command += $" -b:v {streamSettings.Bitrate} -c:v {streamSettings.VideoEncoder} " +
                           $"-c:a {streamSettings.AudioCodec} -b:a {streamSettings.AudioBitrate} " +
                           $"-bufsize {bufferSize}";
            }
            else
            {
                command += " -c copy";  // Copy if no encoding settings provided
            }

            // Tee output for this group to all StreamTargets
            string teeCommand = CreateTeeCommand(group.StreamTargets);
            command += $" -f tee {teeCommand}";

            return command;
        }

        // Function to calculate buffer size as 2x the sum of video and audio bitrates
        private string CalculateBufferSize(StreamSettings streamSettings)
        {
            int videoBitrate = int.Parse(streamSettings.Bitrate.Replace("k", ""));
            int audioBitrate = int.Parse(streamSettings.AudioBitrate.Replace("k", ""));
            int bufferSize = 2 * (videoBitrate + audioBitrate);
            return bufferSize + "k";
        }

        // Creates the tee command for duplicating output to multiple URLs
        private string CreateTeeCommand(List<StreamTarget> streamTargets)
        {
            List<string> teeOutputs = new();

            // Iterate over each StreamTarget and add it to the tee list
            foreach (var streamTarget in streamTargets)
            {
                string fullUrl = streamTarget.GenerateFullUrl();
                teeOutputs.Add($"[f=flv]{fullUrl}");
            }

            // Join all tee outputs with the pipe (|) symbol
            return $"\"{string.Join('|', teeOutputs)}\"";
        }

        // Start the FFmpeg process with the generated arguments
        private Process StartProcess(string ffmpegArgs)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = ffmpegArgs,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            return process;
        }

        // Method to stop the running FFmpeg process
        public void StopFFmpegProcess()
        {
            if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
            {
                _ffmpegProcess.Kill();  // Gracefully kill the process
                _ffmpegProcess.WaitForExit();
            }
        }
    }
}
