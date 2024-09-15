using System.Diagnostics;
using Utilities;

namespace Backend
{
    public class FFmpegService
    {
        // Dictionary to store running FFmpeg processes by output group index
        private readonly Dictionary<int, Process> _ffmpegProcesses = new();

        // Manages the interaction with FFmpeg, including starting and stopping streams.
        public void StartFFmpegProcess(Profile profile)
        {
            int groupCounter = 0; // Counter to uniquely identify each output group

            foreach (var group in profile.OutputGroups)
            {
                // Step 1: Create encoding command for this output group
                string encodingCommand = BuildEncodingCommand(group, profile.IncomingUrl, profile.GeneratePTS);

                // Step 2: Create tee command for output Urls in this group
                string teeCommand = CreateTeeCommand(group.OutputUrls);

                // Step 3: Combine encoding and tee commands into the final FFmpeg command
                string finalCommand = encodingCommand + " -f tee " + teeCommand;

                // Step 4: Start the FFmpeg process for the group
                var process = StartProcess(finalCommand);

                // Store the process using the groupCounter as the key
                _ffmpegProcesses[groupCounter] = process;
                groupCounter++; // Increment the counter for each group
            }
        }

        // Builds the encoding command based on the group's settings
        private string BuildEncodingCommand(OutputGroup group, string incomingUrl, bool generatePTS)
        {
            string ptsOption = generatePTS ? "-fflags +genpts" : string.Empty;
            string command = $"-i {incomingUrl} {ptsOption}";

            if (group.ForwardOriginal)
            {
                // Forward the original stream without re-encoding
                command += " -c copy";
            }
            else if (group.EncodingSettings != null)
            {
                // Use custom encoding settings for the group
                var encodingSettings = group.EncodingSettings;
                string bufferSize = CalculateBufferSize(encodingSettings);

                command += $" -s {encodingSettings.Resolution} -c:v {encodingSettings.VideoEncoder} " +
                           $"-b:v {encodingSettings.Bitrate} -c:a {encodingSettings.AudioCodec} " +
                           $"-b:a {encodingSettings.AudioBitrate} -bufsize {bufferSize}";
            }

            return command;
        }

        // Creates the tee command for duplicating output to multiple Urls
        private string CreateTeeCommand(List<OutputUrl> outputUrls)
        {
            List<string> teeOutputs = new();

            // Iterate over each output Url and add to the tee list
            foreach (var outputUrl in outputUrls)
            {
                string fullUrl = outputUrl.GenerateFullUrl();
                teeOutputs.Add($"[f=flv]{fullUrl}");
            }

            // Join all tee outputs with the pipe (|) symbol
            return $"\"{string.Join('|', teeOutputs)}\"";
        }

        // Function to calculate buffer size as 2x the sum of video and audio bitrates
        private string CalculateBufferSize(Settings encodingSettings)
        {
            int videoBitrate = int.Parse(encodingSettings.Bitrate.Replace("k", ""));
            int audioBitrate = int.Parse(encodingSettings.AudioBitrate.Replace("k", ""));
            int bufferSize = 2 * (videoBitrate + audioBitrate);
            return bufferSize + "k";
        }

        // Start the FFmpeg process
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

        // Method to stop all running FFmpeg processes
        public void StopFFmpegProcess()
        {
            foreach (var process in _ffmpegProcesses.Values)
            {
                if (!process.HasExited)
                {
                    process.Kill(); // Gracefully kill the process
                    process.WaitForExit();
                }
            }

            _ffmpegProcesses.Clear(); // Clear the dictionary after stopping all processes
        }

        // Method to stop a specific process by output group index
        public void StopFFmpegProcess(int groupIndex)
        {
            if (_ffmpegProcesses.ContainsKey(groupIndex))
            {
                var process = _ffmpegProcesses[groupIndex];
                if (!process.HasExited)
                {
                    process.Kill();
                    process.WaitForExit();
                }

                _ffmpegProcesses.Remove(groupIndex); // Remove the stopped process
            }
        }
    }
}
