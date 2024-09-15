namespace Utilities
{
    public class Profile
    {
        public string ProfileName { get; set; } = string.Empty;   // The name of the profile
        public string IncomingUrl { get; set; } = string.Empty;   // The Url where the app will listen for an incoming stream
        public List<OutputGroup> OutputGroups { get; set; } = new List<OutputGroup>();  // 0 or more output groups
        public bool GeneratePTS { get; set; } = false;  // Default to false
        public string Theme { get; set; } = "light";  // Default theme
        public string Language { get; set; } = "en-US";  // Default language

        // Validate all output groups in the profile
        public void Validate()
        {
            foreach (var outputGroup in OutputGroups)
            {
                outputGroup.Validate();  // Ensure each output group is valid
            }
        }
    }

    public class OutputGroup
    {
        public string Name { get; set; } = string.Empty;  // The name of the output group
        public bool ForwardOriginal { get; set; } = false;  // Indicates if the original stream is forwarded without re-encoding
        public List<OutputUrl> OutputUrls { get; set; } = new List<OutputUrl>();  // 0 or more output Urls
        public Settings? EncodingSettings { get; set; }  // Encoding settings if not forwarding the original stream

        // Validation method to check that either ForwardOriginal is true or EncodingSettings exists
        public void Validate()
        {
            if (!ForwardOriginal && EncodingSettings == null)
            {
                throw new InvalidOperationException("Either ForwardOriginal must be true, or EncodingSettings must be provided.");
            }
        }
    }

    public class OutputUrl
    {
        public string Url { get; set; } = string.Empty;  // The base Url of the ingest server
        public string StreamKey { get; set; } = string.Empty;  // Stream key specific to this output Url
        public string Template { get; set; } = string.Empty;  // The Url template for generating the full Url

        // Generates the full Url by replacing the stream key in the template or base Url
        public string GenerateFullUrl()
            {
                return !string.IsNullOrEmpty(Template)
                    ? Template.Replace("{streamKey}", StreamKey)
                    : Url.Replace("{streamKey}", StreamKey);
            }

        // Generates the Url with the stream key masked for display purposes
        public string GenerateUrlWithMaskedKey()
            {
                string maskedKey = new string('*', StreamKey.Length);
                return !string.IsNullOrEmpty(Template)
                    ? Template.Replace("{streamKey}", maskedKey)
                    : Url.Replace("{streamKey}", maskedKey);
            }
    }

    public class Settings
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
