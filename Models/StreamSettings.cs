namespace MagillaStream.Models
{
    public class StreamSettings
    {
        public string VideoEncoder { get; set; } = "libx264";  // Default video encoder
        public string EncoderPreset { get; set; } = "medium";  // Default encoder preset
        public string Resolution { get; set; } = "1920x1080";  // Default resolution
        public int Fps { get; set; } = 30;  // Default frames per second
        public string Bitrate { get; set; } = "6000k";  // Default video bitrate
        public string AudioCodec { get; set; } = "aac";  // Default audio codec
        public string AudioBitrate { get; set; } = "192k";  // Default audio bitrate
    }
}