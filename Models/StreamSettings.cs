using System.Collections.ObjectModel;

namespace MagillaStream.Models
{
    public class StreamSettings
    {
        public string VideoEncoder { get; set; } = string.Empty;
        public string EncoderPreset { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty;
        public int Fps { get; set; } = 30;
        public string Bitrate { get; set; } = string.Empty;
        public string AudioCodec { get; set; } = string.Empty;
        public string AudioBitrate { get; set; } = string.Empty;

        // Collection of available video encoders to be populated
        public ObservableCollection<string> AvailableVideoEncoders { get; }

        // Constructor to accept available encoders
        public StreamSettings(ObservableCollection<string> availableVideoEncoders)
        {
            AvailableVideoEncoders = availableVideoEncoders;
        }
    }
}
