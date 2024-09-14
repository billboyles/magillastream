
using System.IO;
using System.Text.Json;

public class SettingsManager
{
    private static readonly string settingsFilePath = "settings.json";

    // Load settings from a JSON file
    public static AppSettings LoadSettings()
    {
        if (File.Exists(settingsFilePath))
        {
            var settingsJson = File.ReadAllText(settingsFilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(settingsJson);

            // If deserialization returns null, return a new AppSettings object
            return settings ?? new AppSettings();
        }

        return new AppSettings();  // Return default settings if the file doesn't exist
    }

    // Save settings to a JSON file
    public static void SaveSettings(AppSettings settings)
    {
        var settingsJson = JsonSerializer.Serialize(settings);
        File.WriteAllText(settingsFilePath, settingsJson);
    }
}
