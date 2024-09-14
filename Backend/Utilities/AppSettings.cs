namespace Backend.Utilities
{
    public class OutputService
    {
        public required string Url { get; set; }
        public string? StreamKey { get; set; } // Stream key can be optional
    }

    public class EncodingSettings
    {
        public required string Name { get; set; } // Name of the encoding column
        public required string Encoder { get; set; } // Encoder (e.g., libx264)
        public required string Resolution { get; set; } // Resolution (e.g., 1080p)
        public required string Bitrate { get; set; } // Bitrate (e.g., 6000k)
        public required List<OutputService> OutputServices { get; set; } = new List<OutputService>(); // List of output services (URLs and Stream Keys)
    }

    public class AppSettings
    {
        public required string Name { get; set; }
        public required string Encoder { get; set; }
        public required string Resolution { get; set; }
        public required string Bitrate { get; set; }
        public required List<OutputService> OutputServices { get; set; } = new List<OutputService>();

        // OBS Stream URL for the original input
        public string ObsStreamUrl { get; set; } = "rtmp://default-url"; // Default value

        // List to store original stream outputs
        public List<OutputService> OriginalStreamOutputs { get; set; } = new List<OutputService>();

        // Encodings applied to the stream
        public List<EncodingSettings> Encodings { get; set; } = new List<EncodingSettings>();
    }
}
