using System.IO;
using System.Text.Json;

namespace Utilities
{
    public class ProfileManager
    {
        private readonly string _profileDirectory = "profiles";

        // Create a new profile
        public void CreateProfile(string profileName, Profile profile)
        {
            if (!Directory.Exists(_profileDirectory))
            {
                Directory.CreateDirectory(_profileDirectory);
            }

            var profilePath = Path.Combine(_profileDirectory, $"{profileName}.enc");

            if (File.Exists(profilePath))
            {
                throw new InvalidOperationException("Profile already exists.");
            }

            // Serialize to JSON and save to a file
            File.WriteAllText(profilePath, JsonSerializer.Serialize(profile));
        }

        // Load an existing profile
        public Profile LoadProfile(string profileName)
        {
            var profilePath = Path.Combine(_profileDirectory, $"{profileName}.enc");

            if (!File.Exists(profilePath))
            {
                throw new FileNotFoundException("Profile does not exist.");
            }

            // Read and deserialize the profile data
            var profileJson = File.ReadAllText(profilePath);
            var profile = JsonSerializer.Deserialize<Profile>(profileJson);

            if (profile == null)
            {
                throw new InvalidDataException("Failed to load the profile. Deserialization returned null.");
            }

            return profile;
        }
    }
}
