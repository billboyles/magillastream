
public static class UrlGenerator
{
    // Generates the full URL by replacing the stream key
    public static string GenerateFullUrl(string baseUrl, string streamKey)
    {
        var fullUrl = baseUrl.Replace("{streamKey}", streamKey);

        return fullUrl;
    }

    // Generates the URL with the stream key masked for display purposes
    public static string GenerateUrlWithMaskedKey(string baseUrl, string streamKey)
    {
        string maskedKey = new string('*', streamKey.Length);
        string fullUrl = baseUrl.Replace("{streamKey}", maskedKey);

        return fullUrl;
    }
}
