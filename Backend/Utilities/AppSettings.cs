public class OutputService
{
    public required string Url { get; set; }
    public string? StreamKey { get; set; }  // Stream key can be part of the URL or separate
}

public class EncodingSettings
{
    public string? Encoder { get; set; }  // Encoder for re-encoding
    public string? Resolution { get; set; }  // Resolution for re-encoding
    public string? Bitrate { get; set; }  // Bitrate for re-encoding
    public List<OutputService> OutputServices { get; set; } = new List<OutputService>();  // List of output URLs
}

public class AppSettings
{
    public required string ObsStreamUrl { get; set; }  // OBS Stream input URL
    public List<OutputService> OriginalStreamOutputs { get; set; } = new List<OutputService>();  // Output URLs for original stream
    public List<EncodingSettings> Encodings { get; set; } = new List<EncodingSettings>();  // Re-encoded streams with custom settings
}
