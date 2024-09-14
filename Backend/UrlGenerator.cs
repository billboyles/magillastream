public static class UrlGenerator
{
    // Generates the full URL by replacing the {streamKey} placeholder in the template
    public static string GenerateFullUrl(string baseUrl, string streamKey, string template)
    {
        // Use the template if provided, otherwise fall back to the base URL
        var fullUrl = !string.IsNullOrEmpty(template) 
            ? template.Replace("{streamKey}", streamKey) 
            : baseUrl.Replace("{streamKey}", streamKey);

        return fullUrl;
    }

    // Generates the URL with the stream key masked for display purposes
    public static string GenerateUrlWithMaskedKey(string baseUrl, string streamKey, string template)
    {
        // Mask the stream key for display
        string maskedKey = new string('*', streamKey.Length);

        // Use the template if provided, otherwise fall back to the base URL
        var fullUrl = !string.IsNullOrEmpty(template) 
            ? template.Replace("{streamKey}", maskedKey) 
            : baseUrl.Replace("{streamKey}", maskedKey);

        return fullUrl;
    }
}
