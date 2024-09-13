using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Backend.Utilities;

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
        var serializedSettings = $"{settings.ObsStreamUrl}|{settings.StreamKey}|{settings.SelectedService}|{settings.Bitrate}|{settings.SelectedResolution}|{settings.SelectedEncoder}";

        // Generate a unique salt for this encryption
        var salt = GenerateSalt();

        // Encrypt the settings using the salt
        var encryptedData = Encrypt(serializedSettings, _password, salt);

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
                StreamKey = string.Empty,
                SelectedService = "YouTube",
                Bitrate = "6000k", // Default bitrate
                SelectedResolution = "1080p",
                SelectedEncoder = "libx264"
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
        return new AppSettings
        {
            ObsStreamUrl = parts[0],
            StreamKey = parts[1],
            SelectedService = parts[2],
            Bitrate = parts[3],
            SelectedResolution = parts[4],
            SelectedEncoder = parts[5]
        };
    }


    private byte[] Encrypt(string plainText, string password, byte[] salt)
    {
        using (var aes = Aes.Create())
        {
            // Use the modern constructor with hash algorithm and iterations
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
                return ms.ToArray(); // Ensure the encrypted data is properly returned
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
