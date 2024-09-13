using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Backend.Utilities;
using System.Collections.Generic;

public class SettingsManager
{
    private readonly string _password;
    private readonly string _filePath = "settings.enc"; // Encrypted settings file

    public SettingsManager(string password)
    {
        _password = password;
    }

    public void SaveSettings(AppSettings settings)
    {
        // Serialize the settings
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
            sb.Append($"{encoding.Encoder},{encoding.Resolution},{encoding.Bitrate}|");
            foreach (var output in encoding.OutputServices)
            {
                sb.Append($"{output.Url},{output.StreamKey ?? string.Empty};");
            }
            sb.Append("|");
        }

        // Generate a unique salt for this encryption
        var salt = GenerateSalt();

        // Encrypt the settings using the salt
        var encryptedData = Encrypt(sb.ToString(), _password, salt);

        // Combine salt and encrypted data for storage
        var dataToStore = new byte[salt.Length + encryptedData.Length];
        Buffer.BlockCopy(salt, 0, dataToStore, 0, salt.Length);
        Buffer.BlockCopy(encryptedData, 0, dataToStore, salt.Length, encryptedData.Length);

        // Write to file
        File.WriteAllBytes(_filePath, dataToStore);
    }

    public AppSettings LoadSettings()
    {
        if (!File.Exists(_filePath))
        {
            // Provide default values for all required properties
            return new AppSettings
            {
                ObsStreamUrl = string.Empty,
                OriginalStreamOutputs = new List<OutputService>
                {
                    new OutputService { Url = "rtmp://default-url", StreamKey = string.Empty }
                },
                Encodings = new List<EncodingSettings>()
            };
        }

        // Read the stored data
        var dataToLoad = File.ReadAllBytes(_filePath);

        // Extract the salt from the beginning of the file (first 16 bytes)
        var salt = new byte[16];
        Buffer.BlockCopy(dataToLoad, 0, salt, 0, salt.Length);

        // Extract the encrypted data (the rest of the bytes)
        var encryptedData = new byte[dataToLoad.Length - salt.Length];
        Buffer.BlockCopy(dataToLoad, salt.Length, encryptedData, 0, encryptedData.Length);

        // Decrypt the data using the salt and password
        var decryptedData = Decrypt(encryptedData, _password, salt);

        // Deserialize the settings
        var parts = decryptedData.Split('|');
        var obsStreamUrl = parts[0];

        // Deserialize Original Stream Outputs
        var originalOutputs = new List<OutputService>();
        var originalOutputParts = parts[1].Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var outputPart in originalOutputParts)
        {
            var outputParts = outputPart.Split(',');
            originalOutputs.Add(new OutputService { Url = outputParts[0], StreamKey = outputParts[1] });
        }

        // Deserialize Encodings
        var encodings = new List<EncodingSettings>();
        for (int i = 2; i < parts.Length; i += 2)
        {
            var encodingParts = parts[i].Split(',');
            var outputParts = parts[i + 1].Split(';', StringSplitOptions.RemoveEmptyEntries);

            var encoding = new EncodingSettings
            {
                Encoder = encodingParts[0],
                Resolution = encodingParts[1],
                Bitrate = encodingParts[2],
                OutputServices = new List<OutputService>()
            };

            foreach (var outputPart in outputParts)
            {
                var outputServiceParts = outputPart.Split(',');
                encoding.OutputServices.Add(new OutputService { Url = outputServiceParts[0], StreamKey = outputServiceParts[1] });
            }

            encodings.Add(encoding);
        }

        return new AppSettings
        {
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
        var salt = new byte[16]; // 128-bit salt
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        return salt;
    }
}
