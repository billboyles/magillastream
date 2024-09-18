using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Text;

namespace MagillaStream.Utilities
{
    public static class EncryptionUtils
    {
        private static readonly string EncryptionKey = "your-encryption-key-here"; // Use a secure, randomly generated key

        // Encrypt the plain text using Bouncy Castle AES in CBC mode
        public static string Encrypt(string plainText)
        {
            byte[] key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32)); // Pad the key to 32 bytes (for AES-256)
            byte[] input = Encoding.UTF8.GetBytes(plainText); // Convert the plain text to bytes

            var cipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(new AesEngine())); // Use CBC mode with AES
            cipher.Init(true, new KeyParameter(key)); // Initialize the cipher for encryption

            byte[] output = new byte[cipher.GetOutputSize(input.Length)];
            int len = cipher.ProcessBytes(input, 0, input.Length, output, 0);
            cipher.DoFinal(output, len); // Finalize encryption

            return Convert.ToBase64String(output); // Convert the encrypted bytes to a Base64 string
        }

        // Decrypt the encrypted text using Bouncy Castle AES in CBC mode
        public static string Decrypt(string encryptedText)
        {
            byte[] key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32)); // Pad the key to 32 bytes
            byte[] input = Convert.FromBase64String(encryptedText); // Convert the Base64 string back to bytes

            var cipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(new AesEngine())); // Use CBC mode with AES
            cipher.Init(false, new KeyParameter(key)); // Initialize the cipher for decryption

            byte[] output = new byte[cipher.GetOutputSize(input.Length)];
            int len = cipher.ProcessBytes(input, 0, input.Length, output, 0);
            cipher.DoFinal(output, len); // Finalize decryption

            return Encoding.UTF8.GetString(output).TrimEnd('\0'); // Convert decrypted bytes back to a string and remove padding
        }
    }
}