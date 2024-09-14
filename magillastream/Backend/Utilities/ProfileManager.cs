using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Backend.Utilities;

public class ProfileManager
{
    private readonly string _profileDirectory = "profiles";  // Directory to store profiles

    // Fetch the list of profiles from the directory
    public List<string> GetProfiles()
    {
        if (!Directory.Exists(_profileDirectory))
        {
            Directory.CreateDirectory(_profileDirectory);
        }

        // Get profile files and remove the file extension for display
        return Directory.GetFiles(_profileDirectory, "*.enc")
            .Select(profile => Path.GetFileNameWithoutExtension(profile))
            .Where(profile => profile != null) // Ensure no null values
            .ToList()!;
    }

    // Create a new profile with the provided name and password
    public void CreateProfile(string profileName, string password)
    {
        // Ensure profile directory exists
        if (!Directory.Exists(_profileDirectory))
        {
            Directory.CreateDirectory(_profileDirectory);
        }

        // Verify profile does not already exist
        var profilePath = Path.Combine(_profileDirectory, $"{profileName}.enc");
        if (File.Exists(profilePath))
        {
            Logger.LogError($"Profile {profileName} already exists.");
            throw new InvalidOperationException("Profile already exists.");
        }

        // Create a new settings manager for this profile
        var settingsManager = new SettingsManager(password, profileName);

        // Define default settings for the new profile
        var settings = new AppSettings
        {
            Name = profileName, // Set profile name
            Encoder = "libx264", // Default encoder
            Resolution = "1080p", // Default resolution
            Bitrate = "6000k", // Default bitrate
            OutputServices = new List<OutputService>(), // No output services yet
            ObsStreamUrl = "rtmp://default-url", // Default OBS stream URL
            OriginalStreamOutputs = new List<OutputService>(), // No outputs for now
            Encodings = new List<EncodingSettings>() // Empty list of encodings
        };

        // Save the newly created profile settings
        try
        {
            settingsManager.SaveSettings(settings);
            Logger.LogInfo($"Profile {profileName} created successfully.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to create profile {profileName}: {ex.Message}");
            throw;
        }
    }

    // Delete an existing profile
    public void DeleteProfile(string profileName)
    {
        // Construct the file path for the profile
        var filePath = Path.Combine(_profileDirectory, $"{profileName}.enc");

        // If the file exists, delete it
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Logger.LogInfo($"Profile {profileName} deleted successfully.");
        }
        else
        {
            Logger.LogWarning($"Profile {profileName} not found for deletion.");
        }
    }
}
