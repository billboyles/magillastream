using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        var ffmpegService = new FFmpegService();

        // List of output services (URL, Stream Key, Re-encode flag)
        var outputServices = new List<Tuple<string, string, bool>>()
        {
            Tuple.Create("rtmp://a.rtmp.youtube.com/live2", "your-youtube-stream-key", false),
            Tuple.Create("rtmp://live.twitch.tv/app", "your-twitch-stream-key", true)
        };

        // Start the stream
        ffmpegService.StartStream("rtmp://obs-stream-url", outputServices);

        // Let the stream run for 10 seconds before stopping
        System.Threading.Thread.Sleep(10000);
        ffmpegService.StopStream();
    }
}
