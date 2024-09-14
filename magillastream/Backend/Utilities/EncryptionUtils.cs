using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Backend.Utilities
{
    public static class EncryptionUtils
    {
        // AES Encryption
        public static string Encrypt(string plainText, string password)
        {
            try
            {
                // Generate a 16-byte salt
                byte[] salt = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                using (var aes = Aes.Create())
                {
                    // Derive the key and IV using the password and salt
                    var key = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
                    aes.Key = key.GetBytes(32);
                    aes.IV = key.GetBytes(16);
                    aes.Padding = PaddingMode.PKCS7;

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (var ms = new MemoryStream())
                    {
                        // Write the salt first
                        ms.Write(salt, 0, salt.Length);

                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        using (var writer = new StreamWriter(cs))
                        {
                            writer.Write(plainText);
                        }

                        // Log encrypted data before encoding to Base64
                        byte[] encryptedData = ms.ToArray();
                        Logger.LogDebug($"Encrypted data (before Base64): {Convert.ToBase64String(encryptedData)}");

                        // Return Base64-encoded result
                        return Convert.ToBase64String(encryptedData);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Encryption failed: {ex.Message}");
                throw new InvalidOperationException("Failed to encrypt profile data.");
            }
        }

        // AES Decryption
        public static string Decrypt(string cipherText, string password)
        {
            try
            {
                // Decode the Base64-encoded cipher text
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                // Extract the salt (first 16 bytes)
                byte[] salt = new byte[16];
                Array.Copy(cipherBytes, 0, salt, 0, salt.Length);

                using (var aes = Aes.Create())
                {
                    // Derive the key and IV using the password and extracted salt
                    var key = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
                    aes.Key = key.GetBytes(32);
                    aes.IV = key.GetBytes(16);
                    aes.Padding = PaddingMode.PKCS7;

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    using (var ms = new MemoryStream(cipherBytes, 16, cipherBytes.Length - 16))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        try
                        {
                            using (var reader = new StreamReader(cs))
                            {
                                Logger.LogDebug($"Decryption successful. Returning decrypted data.");
                                return reader.ReadToEnd();
                            }
                        }
                        catch (CryptographicException ex)
                        {
                            Logger.LogError($"Decryption failed: {ex.Message}. Possible padding error or data corruption.");
                            throw new InvalidOperationException("Failed to decrypt profile data. Please check your password or the profile's integrity.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Decryption failed: {ex.Message}");
                throw new InvalidOperationException("Failed to decrypt profile data.");
            }
        }

        private static byte[] GenerateSalt()
        {
            var salt = new byte[16]; // 128-bit salt
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }
    }
}