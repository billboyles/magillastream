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
    private string _filePath => $"settings_{_profileName}.enc"; // Profile-specific settings file

    public SettingsManager(string password, string profileName)
    {
        _password = password;
        _profileName = profileName;
    }

    // Save settings to a file
    public void SaveSettings(AppSettings settings)
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
            sb.Append("|");
        }

        var salt = GenerateSalt();
        var encryptedData = Encrypt(sb.ToString(), _password, salt);
        var dataToStore = new byte[salt.Length + encryptedData.Length];
        Buffer.BlockCopy(salt, 0, dataToStore, 0, salt.Length);
        Buffer.BlockCopy(encryptedData, 0, dataToStore, salt.Length, encryptedData.Length);

        File.WriteAllBytes(_filePath, dataToStore);
    }

    // Load settings from a file
    public AppSettings LoadSettings()
    {
        if (!File.Exists(_filePath))
        {
            return new AppSettings
            {
                Name = "Default Profile",
                Encoder = "libx264",
                Resolution = "1080p",
                Bitrate = "6000k",
                OutputServices = new List<OutputService>(),
                ObsStreamUrl = "rtmp://default-url",
                OriginalStreamOutputs = new List<OutputService>(),
                Encodings = new List<EncodingSettings>()
            };
        }

        var dataToLoad = File.ReadAllBytes(_filePath);
        var salt = new byte[16];
        Buffer.BlockCopy(dataToLoad, 0, salt, 0, salt.Length);
        var encryptedData = new byte[dataToLoad.Length - salt.Length];
        Buffer.BlockCopy(dataToLoad, salt.Length, encryptedData, 0, encryptedData.Length);
        var decryptedData = Decrypt(encryptedData, _password, salt);

        var parts = decryptedData.Split('|');
        var obsStreamUrl = parts[0];

        var originalOutputs = new List<OutputService>();
        var originalOutputParts = parts[1].Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var outputPart in originalOutputParts)
        {
            var outputParts = outputPart.Split(',');
            originalOutputs.Add(new OutputService { Url = outputParts[0], StreamKey = outputParts.Length > 1 ? outputParts[1] : string.Empty });
        }

        var encodings = new List<EncodingSettings>();
        for (int i = 2; i < parts.Length; i += 2)
        {
            var encodingParts = parts[i].Split(',');
            var outputParts = parts[i + 1].Split(';', StringSplitOptions.RemoveEmptyEntries);

            var encoding = new EncodingSettings
            {
                Name = encodingParts[0],
                Encoder = encodingParts[1],
                Resolution = encodingParts[2],
                Bitrate = encodingParts[3],
                OutputServices = new List<OutputService>()
            };

            foreach (var outputPart in outputParts)
            {
                var outputServiceParts = outputPart.Split(',');
                encoding.OutputServices.Add(new OutputService { Url = outputServiceParts[0], StreamKey = outputServiceParts.Length > 1 ? outputServiceParts[1] : string.Empty });
            }

            encodings.Add(encoding);
        }

        return new AppSettings
        {
            Name = "Loaded Profile",
            Encoder = "libx264",
            Resolution = "1080p",
            Bitrate = "6000k",
            OutputServices = new List<OutputService>(),
            ObsStreamUrl = obsStreamUrl,
            OriginalStreamOutputs = originalOutputs,
            Encodings = encodings
        };
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
            using (var reader = new StreamReader(cs))
            {
                return reader.ReadToEnd();
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
