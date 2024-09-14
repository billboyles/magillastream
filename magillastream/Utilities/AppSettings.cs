using System;

public class AppSettings
{
    public string Theme { get; set; } = "light";   // Can be "light" or "dark"
    public bool GeneratePTS { get; set; } = false; // Whether to enable PTS generation
    public string LastUsedProfile { get; set; } = "";  // Store the last profile used

    // Additional settings as required
    public string Language { get; set; } = "en-US";  // Default language for localization

    // Method to display the current settings
    public void DisplaySettings()
    {
        Console.WriteLine("Current App Settings:");
        Console.WriteLine($"Theme: {Theme}");
        Console.WriteLine($"Generate PTS: {GeneratePTS}");
        Console.WriteLine($"Last Used Profile: {LastUsedProfile}");
        Console.WriteLine($"Language: {Language}");
    }
}
