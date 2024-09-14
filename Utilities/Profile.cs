namespace Utilities
{
    public class Profile
    {
        public string ProfileName { get; set; } = string.Empty;
        public string IncomingUrl { get; set; } = string.Empty;
        public List<OutputGroup> OutputGroups { get; set; } = new List<OutputGroup>();  // 0 or more output groups
        public bool GeneratePTS { get; set; } = false;  // Default to false
        public string Theme { get; set; } = "light";  // Default theme
        public string Language { get; set; } = "en-US";  // Default language
    }

    public class OutputGroup
    {
        public string Name { get; set; } = string.Empty;  // The name of the output group
        public string EncodingSettings { get; set; } = string.Empty;  // Empty if using incoming stream settings
        public List<OutputURL> OutputUrls { get; set; } = new List<OutputURL>();  // List of output URLs
        public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>()  // Settings such as bitrate
        {
            { "bitrate", "6000k" }  // Default bitrate
        };
    }

    public class OutputURL
    {
        public string Url { get; set; } = string.Empty;  // The URL of the ingest server
        public string StreamKey { get; set; } = string.Empty;  // Stream key specific to this output URL
        public bool UseBackup { get; set; } = false;  // Whether to use a backup URL
        public Dictionary<string, string> QueryStrings { get; set; } = new Dictionary<string, string>();  // Optional query strings
    }
}
