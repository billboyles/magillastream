using System;
using System.IO;
using System.Text.Json;

namespace MagillaStream.Models
{
    public class AppSettings
    {
        public string LastUsedProfile { get; set; } = "";
        public bool FirstLaunch { get; set; } = true; 

        private static readonly string SettingsFile = "appsettings.json";

        // Save the current settings to a file
        public void Save()
        {
            try
            {
                var settingsJson = JsonSerializer.Serialize(this);
                File.WriteAllText(SettingsFile, settingsJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        // Load the settings from a file, return default if no file is found
        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    var settingsJson = File.ReadAllText(SettingsFile);
                    return JsonSerializer.Deserialize<AppSettings>(settingsJson) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }

            return new AppSettings(); // Return default settings if no file is found or if there's an error
        }
    }
}
