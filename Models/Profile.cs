namespace MagillaStream.Models
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
}
