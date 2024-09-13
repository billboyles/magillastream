public class OutputService
{
    public required string Url { get; set; }
    public string? StreamKey { get; set; }  // Stream key can be part of the URL or separate
}

public class EncodingSettings
{
    public string Name { get; set; } // New property for storing the name of the encoding column
    public string Encoder { get; set; } // Encoder (e.g., libx264)
    public string Resolution { get; set; } // Resolution (e.g., 1080p)
    public string Bitrate { get; set; } // Bitrate (e.g., 6000k)
    public List<OutputService> OutputServices { get; set; } // List of output services (URLs and Stream Keys)
}


public class AppSettings
{
    public required string ObsStreamUrl { get; set; }  // OBS Stream input URL
    public List<OutputService> OriginalStreamOutputs { get; set; } = new List<OutputService>();  // Output URLs for original stream
    public List<EncodingSettings> Encodings { get; set; } = new List<EncodingSettings>();  // Re-encoded streams with custom settings
}
