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
                    // Get the full URL using the OutputURL class method
                    var fullUrl = outputUrl.GenerateFullUrl();

                    // Build FFmpeg command using the group's encoding settings and profile's incoming URL
                    var ffmpegArgs = BuildFFmpegCommand(group, profile.IncomingUrl, fullUrl, profile.GeneratePTS);

                    // Start the FFmpeg process
                    StartProcess(ffmpegArgs);
                }
            }
        }

        private string BuildFFmpegCommand(OutputGroup group, string incomingUrl, string outputUrl, bool generatePTS)
        {
            string command;
            string bufferSize = CalculateBufferSize(group.EncodingSettings);

            // PTS generation logic
            string ptsOption = generatePTS ? "-fflags +genpts" : string.Empty;

            if (group.ForwardOriginal)
            {
                // Forward the original stream without re-encoding
                command = $"-i {incomingUrl} -c copy {ptsOption} -f flv {outputUrl}";
            }
            else if (group.EncodingSettings != null)
            {
                // Use custom encoding settings from the group
                var encodingSettings = group.EncodingSettings;
                command = $"-i {incomingUrl} -c:v {encodingSettings.VideoEncoder} " +
                          $"-preset {encodingSettings.EncoderPreset} -s {encodingSettings.Resolution} " +
                          $"-r {encodingSettings.Fps} -b:v {encodingSettings.Bitrate} " +
                          $"-c:a {encodingSettings.AudioCodec} -b:a {encodingSettings.AudioBitrate} " +
                          $"-bufsize {bufferSize} {ptsOption} -f flv {outputUrl}";
            }
            else
            {
                // Fallback to default behavior if EncodingSettings is missing
                command = $"-i {incomingUrl} -c copy {ptsOption} -f flv {outputUrl}";
            }

            return command;
        }

        // Function to calculate buffer size as 2x the sum of video and audio bitrates
        private string CalculateBufferSize(Settings? encodingSettings)
        {
            if (encodingSettings != null)
            {
                // Extract video and audio bitrates (remove "k" and convert to integer)
                int videoBitrate = int.Parse(encodingSettings.Bitrate.Replace("k", ""));
                int audioBitrate = int.Parse(encodingSettings.AudioBitrate.Replace("k", ""));

                // Buffer size is 2x the sum of video and audio bitrates
                int bufferSize = 2 * (videoBitrate + audioBitrate);
                return bufferSize + "k";
            }

            // Default buffer size if no settings provided
            return "12000k"; // Default 12Mbps buffer size
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
