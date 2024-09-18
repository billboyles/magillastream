namespace MagillaStream.Models
{
    public class OutputGroup
    {
        public string Name { get; set; } = string.Empty;  // The name of the output group
        public bool ForwardOriginal { get; set; } = false;  // Indicates if the original stream is forwarded without re-encoding
        public List<StreamTarget> StreamTargets { get; set; } = new List<StreamTarget>();  // 0 or more output Urls
        public StreamSettings? StreamSettings { get; set; }  // Encoding settings if not forwarding the original stream

        // Validation method to check that either ForwardOriginal is true or EncodingSettings exists
        public void Validate()
        {
            if (!ForwardOriginal && StreamSettings == null)
            {
                throw new InvalidOperationException("Either ForwardOriginal must be true, or EncodingSettings must be provided.");
            }
        }
    }
}