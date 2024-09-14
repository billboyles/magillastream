using System.Diagnostics;
using Utilities;

namespace Backend
{
    public class FFmpegService
    {
        // Manages the interaction with FFmpeg, including starting and stopping streams.
        public void StartFFmpegProcess(Profile profile)
        {
            foreach (var group in profile.OutputGroups)
            {
                // Iterate through each output URL in the group
                foreach (var outputUrl in group.OutputUrls)
                {
                    // Get the full URL, stream key decrypted internally
                    var fullUrl = UrlGenerator.GenerateFullUrl(outputUrl.Url, outputUrl.StreamKey, outputUrl.UseBackup);

                    // Build FFmpeg command using the group's encoding settings and profile's incoming URL
                    var ffmpegArgs = BuildFFmpegCommand(group, profile.IncomingUrl, fullUrl);

                    // Start the FFmpeg process
                    StartProcess(ffmpegArgs);
                }
            }
        }

        private string BuildFFmpegCommand(OutputGroup group, string incomingUrl, string outputUrl)
        {
            // Get the bitrate from the group settings
            string bitrate = group.Settings.ContainsKey("bitrate") ? group.Settings["bitrate"] : "6000k";  // Default to 6000k if not provided

            // Construct the FFmpeg command based on encoding settings and stream URLs
            if (string.IsNullOrEmpty(group.EncodingSettings))
            {
                // Use the same encoding as the incoming stream
                return $"-i {incomingUrl} -c copy -b:v {bitrate} -f flv {outputUrl}";
            }
            else
            {
                // Apply custom encoding settings
                return $"-i {incomingUrl} {group.EncodingSettings} -b:v {bitrate} -f flv {outputUrl}";
            }
        }

        private void StartProcess(string ffmpegArgs)
        {
            // Start the FFmpeg process with the generated command
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
        }
    }
}
