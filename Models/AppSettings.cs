using System;
using System.IO;
using System.Text.Json;
using MagillaStream.Utilities;

namespace MagillaStream.Models
{
    public class AppSettings
    {
        private static readonly Lazy<AppSettings> _instance = new Lazy<AppSettings>(() => Load());
        public static AppSettings Instance => _instance.Value;

        // Properties
        public string LastUsedProfile { get; set; } = "";
        public bool FirstLaunch { get; set; } = true;

        private static readonly string SettingsDir = "Settings";
        private static readonly string SettingsFile = "Appsettings.json";
        private static readonly string SettingsPath = Path.Combine(SettingsDir, SettingsFile);

        // Parameterless constructor for deserialization
        public AppSettings() { }

        private static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var settingsJson = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<AppSettings>(settingsJson);
                }
                else
                {
                    var defaultSettings = new AppSettings();
                    defaultSettings.Save();
                    return defaultSettings;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error loading settings: {ex.Message}");
                return new AppSettings();
            }
        }

        public void Save()
        {
            try
            {
                var settingsJson = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, settingsJson);
                Logger.Info($"Settings saved at {SettingsPath} with LastUsedProfile: {LastUsedProfile}, FirstLaunch: {FirstLaunch}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error saving settings: {ex.Message}");
            }
        }
    }
}
