using System.Text.Json.Serialization;
using System.Reactive;
using ReactiveUI;


namespace MagillaStream.Models
{
    public class StreamTarget
    {
        public string Url { get; set; } = string.Empty;  // The base Url of the ingest server
        public string StreamKey { get; set; } = string.Empty;  // Stream key specific to this output Url
        public string Template { get; set; } = string.Empty;  // The Url template for generating the full Url

        // Proxy for the RemoveStreamTargetCommand from MainWindowViewModel
        [JsonIgnore]
        public ReactiveCommand<StreamTarget, Unit> RemoveStreamTargetCommand { get; set; }

        // Constructor with a command proxy
        public StreamTarget(ReactiveCommand<StreamTarget, Unit> removeStreamTargetCommand)
        {
            RemoveStreamTargetCommand = removeStreamTargetCommand;
        }

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
}
