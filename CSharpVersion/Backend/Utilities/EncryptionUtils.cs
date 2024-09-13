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
            using var aes = Aes.Create();
            var salt = GenerateSalt();
            var key = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            aes.Key = key.GetBytes(32);
            aes.IV = key.GetBytes(16);

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            // Combine salt and encrypted data for storage
            var encryptedData = ms.ToArray();
            var dataToStore = new byte[salt.Length + encryptedData.Length];
            Buffer.BlockCopy(salt, 0, dataToStore, 0, salt.Length);
            Buffer.BlockCopy(encryptedData, 0, dataToStore, salt.Length, encryptedData.Length);

            return Convert.ToBase64String(dataToStore);
        }

        // AES Decryption
        public static string Decrypt(string cipherText, string password)
        {
            var dataToLoad = Convert.FromBase64String(cipherText);

            // Extract the salt from the beginning of the data (first 16 bytes)
            var salt = new byte[16];
            Buffer.BlockCopy(dataToLoad, 0, salt, 0, salt.Length);

            // Extract the encrypted data (the rest of the bytes)
            var encryptedData = new byte[dataToLoad.Length - salt.Length];
            Buffer.BlockCopy(dataToLoad, salt.Length, encryptedData, 0, encryptedData.Length);

            using var aes = Aes.Create();
            var key = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            aes.Key = key.GetBytes(32);
            aes.IV = key.GetBytes(16);

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(encryptedData);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
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
