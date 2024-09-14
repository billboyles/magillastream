
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Backend.Utilities;
using System.Collections.Generic;

public class SettingsManager
{
    private readonly string _password;
    private readonly string _profileName;
    private string _filePath => $"profiles/{_profileName}.enc"; // Profile-specific settings file

    public SettingsManager(string password, string profileName)
    {
        _password = password;
        _profileName = profileName;
    }

    // Save settings to a file
    public void SaveSettings(AppSettings settings)
    {
        try
        {
            var sb = new StringBuilder();

            // OBS Stream URL
            sb.Append($"{settings.ObsStreamUrl}|");

            // Original Stream Outputs
            foreach (var output in settings.OriginalStreamOutputs)
            {
                sb.Append($"{output.Url},{output.StreamKey ?? string.Empty};");
            }
            sb.Append("|");

            // Encodings
            foreach (var encoding in settings.Encodings)
            {
                sb.Append($"{encoding.Name},{encoding.Encoder},{encoding.Resolution},{encoding.Bitrate}|");
                foreach (var output in encoding.OutputServices)
                {
                    sb.Append($"{output.Url},{output.StreamKey ?? string.Empty};");
                }
                sb.Append(";");
            }
            sb.Append("|");

            // Salt and Encryption
            var salt = GenerateSalt();
            var encryptedData = Encrypt(sb.ToString(), _password, salt);
            var dataToStore = new byte[salt.Length + encryptedData.Length];
            Buffer.BlockCopy(salt, 0, dataToStore, 0, salt.Length);
            Buffer.BlockCopy(encryptedData, 0, dataToStore, salt.Length, encryptedData.Length);

            Logger.LogInfo($"Saving settings to path: {_filePath}");
            File.WriteAllBytes(_filePath, dataToStore);
            Logger.LogInfo($"Settings saved successfully at: {_filePath}");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to save settings for profile {_profileName}: {ex.Message}");
            throw new InvalidOperationException("Failed to save profile settings.");
        }
    }
    // Load settings from a file
    public AppSettings LoadSettings()
    {
        try
        {
            var data = File.ReadAllBytes(_filePath);

            var salt = new byte[16];
            Buffer.BlockCopy(data, 0, salt, 0, salt.Length);

            var encryptedData = new byte[data.Length - salt.Length];
            Buffer.BlockCopy(data, salt.Length, encryptedData, 0, encryptedData.Length);

            var decryptedData = Decrypt(encryptedData, _password, salt);
            var parts = decryptedData.Split('|');

            // Ensure that we have enough parts before accessing them
            if (parts.Length < 3)
            {
                Logger.LogError("Decrypted profile data is incomplete.");
                throw new InvalidOperationException("Failed to load profile settings due to incomplete data.");
            }

            // Parse the OBS stream URL and output services
            var obsStreamUrl = parts[0];
            var originalOutputs = new List<OutputService>();
            var originalParts = parts[1].Split(';');
            foreach (var part in originalParts)
            {
                if (!string.IsNullOrWhiteSpace(part))
                {
                    var outputDetails = part.Split(',');
                    originalOutputs.Add(new OutputService { Url = outputDetails[0], StreamKey = outputDetails.Length > 1 ? outputDetails[1] : string.Empty });
                }
            }

            var encodings = new List<EncodingSettings>();
            for (var i = 2; i < parts.Length; i += 2)
            {
                var encodingDetails = parts[i].Split(',');
                if (encodingDetails.Length < 4)
                {
                    Logger.LogWarning("Incomplete encoding data detected.");
                    continue; // Skip incomplete encoding data
                }

                var encoding = new EncodingSettings
                {
                    Name = encodingDetails[0],
                    Encoder = encodingDetails[1],
                    Resolution = encodingDetails[2],
                    Bitrate = encodingDetails[3],
                    OutputServices = new List<OutputService>()
                };

                if (i + 1 < parts.Length)
                {
                    var encodingOutputs = parts[i + 1].Split(';');
                    foreach (var output in encodingOutputs)
                    {
                        if (!string.IsNullOrWhiteSpace(output))
                        {
                            var outputDetails = output.Split(',');
                            encoding.OutputServices.Add(new OutputService { Url = outputDetails[0], StreamKey = outputDetails.Length > 1 ? outputDetails[1] : string.Empty });
                        }
                    }
                }

                encodings.Add(encoding);
            }

            Logger.LogInfo($"Settings loaded successfully for profile {_profileName}");
            return new AppSettings
            {
                Name = "Default Profile",
                Encoder = "libx264",
                Resolution = "1080p",
                Bitrate = "6000k",
                OutputServices = new List<OutputService>(),
                ObsStreamUrl = obsStreamUrl,
                OriginalStreamOutputs = originalOutputs,
                Encodings = encodings
            };
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to load settings for profile {_profileName}: {ex.Message}");
            throw new InvalidOperationException("Failed to load profile settings.");
        }
    }

    private byte[] Encrypt(string plainText, string password, byte[] salt)
    {
        using (var aes = Aes.Create())
        {
            var key = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            aes.Key = key.GetBytes(32);
            aes.IV = key.GetBytes(16);

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var writer = new StreamWriter(cs))
                {
                    writer.Write(plainText);
                }
                return ms.ToArray();
            }
        }
    }

    private string Decrypt(byte[] cipherText, string password, byte[] salt)
    {
        using (var aes = Aes.Create())
        {
            var key = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            aes.Key = key.GetBytes(32);
            aes.IV = key.GetBytes(16);

            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream(cipherText))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            {
                try
                {
                    using (var reader = new StreamReader(cs))
                    {
                        return reader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Decryption failed for profile {_profileName}: {ex.Message}");
                    throw new InvalidOperationException("Failed to decrypt profile data. Please check your password or the profile's integrity.");
                }
            }
        }
    }

    private byte[] GenerateSalt()
    {
        var salt = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        return salt;
    }
}
