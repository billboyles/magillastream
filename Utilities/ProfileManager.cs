using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MagillaStream.Models;

namespace MagillaStream.Utilities
{
    public class ProfileManager
    {
        private readonly string _profileDirectory = "Profiles";

        // Create a new profile
        public void CreateProfile(Profile profile)
        {
            try
            {
                if (!Directory.Exists(_profileDirectory))
                {
                    Directory.CreateDirectory(_profileDirectory);
                    Logger.Info($"Profile directory created at {_profileDirectory}");
                }

                var profilePath = Path.Combine(_profileDirectory, $"{profile.ProfileName}.json");

                if (File.Exists(profilePath))
                {
                    Logger.Warning($"Profile creation failed. Profile {profile.ProfileName} already exists.");
                    throw new InvalidOperationException("Profile already exists.");
                }

                // Serialize to JSON and save to a file
                File.WriteAllText(profilePath, JsonSerializer.Serialize(profile));
                Logger.Info($"Profile {profile.ProfileName} created successfully at {profilePath}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error creating profile {profile.ProfileName}: {ex.Message}");
                throw;
            }
        }

        // Load an existing profile
        public Profile LoadProfile(string profileName)
        {
            try
            {
                var profilePath = Path.Combine(_profileDirectory, $"{profileName}.json");

                if (!File.Exists(profilePath))
                {
                    Logger.Warning($"LoadProfile failed. Profile {profileName} not found at {profilePath}");
                    throw new FileNotFoundException("Profile does not exist.");
                }

                // Read and deserialize the profile data
                var profileJson = File.ReadAllText(profilePath);
                var profile = JsonSerializer.Deserialize<Profile>(profileJson);

                if (profile == null)
                {
                    Logger.Error($"Failed to deserialize profile {profileName}. Data is invalid.");
                    throw new InvalidDataException("Failed to load the profile. Deserialization returned null.");
                }

                Logger.Info($"Profile {profileName} loaded successfully from {profilePath}");
                return profile;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error loading profile {profileName}: {ex.Message}");
                throw;
            }
        }

        // Save an existing profile (overwrite if it already exists)
        public void SaveProfile(Profile profile)
        {
            try
            {
                var profilePath = Path.Combine(_profileDirectory, $"{profile.ProfileName}.json");

                if (!Directory.Exists(_profileDirectory))
                {
                    Directory.CreateDirectory(_profileDirectory);
                    Logger.Info($"Profile directory created at {_profileDirectory}");
                }

                // Serialize and overwrite the profile
                File.WriteAllText(profilePath, JsonSerializer.Serialize(profile));
                Logger.Info($"Profile {profile.ProfileName} saved successfully at {profilePath}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error saving profile {profile.ProfileName}: {ex.Message}");
                throw;
            }
        }

        public List<string> GetProfilesList()
        {
            try
            {
                if (!Directory.Exists(_profileDirectory))
                {
                    Directory.CreateDirectory(_profileDirectory);
                    Logger.Info($"Profile directory created at {_profileDirectory}");
                }

                string[] profileFiles = Directory.GetFiles(_profileDirectory, "*.json");
                List<string> profileNames = new List<string>();

                foreach (string profileFile in profileFiles)
                {
                    string profileName = Path.GetFileNameWithoutExtension(profileFile);
                    profileNames.Add(profileName);
                }

                Logger.Info($"Found {profileNames.Count} profiles in the profile directory.");
                return profileNames;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error retrieving profile list: {ex.Message}");
                throw;
            }
        }

        // Delete a profile
        public void DeleteProfile(string profileName)
        {
            try
            {
                var profilePath = Path.Combine(_profileDirectory, $"{profileName}.json");

                if (File.Exists(profilePath))
                {
                    File.Delete(profilePath);
                    Logger.Info($"Profile {profileName} deleted successfully.");
                }
                else
                {
                    Logger.Warning($"Profile {profileName} not found for deletion.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error deleting profile {profileName}: {ex.Message}");
                throw;
            }
        }
    }
}
