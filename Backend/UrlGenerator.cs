
public static class UrlGenerator
{
    // Generates the full URL by replacing the stream key
    public static string GenerateFullUrl(string baseUrl, string streamKey, bool useBackup)
    {
        var fullUrl = baseUrl.Replace("{streamKey}", streamKey);

        if (useBackup)
        {
            fullUrl += "?backup=1";
        }

        return fullUrl;
    }

    // Generates the URL with the stream key masked for display purposes
    public static string GenerateUrlWithMaskedKey(string baseUrl, string streamKey, bool useBackup)
    {
        string maskedKey = new string('*', streamKey.Length);
        string fullUrl = baseUrl.Replace("{streamKey}", maskedKey);

        if (useBackup)
        {
            fullUrl += "?backup=1";
        }

        return fullUrl;
    }
}
