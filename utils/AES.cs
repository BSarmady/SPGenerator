using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Crypto {

    /// <summary>
    /// AES encryption/Decryption class
    /// </summary>
    public class AES {

        #region public static byte[] Encrypt(...)
        /// <summary>
        /// Encrypts input bytes and returns encrypted data
        /// </summary>
        /// <param name="inBytes">Encrypted Input bytes</param>
        /// <param name="Key">Key to use for encryption process</param>
        /// <param name="IV">Initial vector</param>
        /// <returns>Encrypted data</returns>
        public static byte[] Encrypt(byte[] inBytes, byte[] Key, byte[] IV) {
            using (Aes aes = Aes.Create()) {
                aes.Padding = PaddingMode.Zeros;
                aes.Mode = CipherMode.CBC;
                aes.Key = Key; // Key Size 128
                aes.IV = IV;
                using (MemoryStream memoryStream = new MemoryStream()) {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(Key, IV), CryptoStreamMode.Write)) {
                        cryptoStream.Write(inBytes, 0, inBytes.Length);
                        cryptoStream.Close();
                        return memoryStream.ToArray();
                    }
                }
            }
        }
        #endregion

        #region public static string Encrypt(...)
        /// <summary>
        /// Encrypts input string and returns encrypted data in base64
        /// </summary>
        /// <param name="inString">Raw text input</param>
        /// <param name="B64Key">Encryption key in Base64</param>
        /// <param name="B64IV">Initial vector in Base64</param>
        /// <returns>Encrypted data in base64 format</returns>
        public static string Encrypt(string inString, string B64Key, string B64IV = "") {
            return Convert.ToBase64String(
                Encrypt(
                    Encoding.UTF8.GetBytes(inString),
                    Convert.FromBase64String(B64Key),
                    B64IV == "" ? new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } : Convert.FromBase64String(B64IV)
                    )
                );
        }
        #endregion

        #region public static string Encrypt(...)
        /// <summary>
        /// Encrypts input string and returns encrypted data in base64
        /// </summary>
        /// <param name="inString">Raw text input</param>
        /// <param name="Key">Encryption key in Base64</param>
        /// <param name="IV">Initial vector in byte, if null, default will be used</param>
        /// <returns>Encrypted data in base64 format</returns>
        public static string Encrypt(string inString, byte[] Key, byte[] IV = null) {
            if (IV == null)
                IV = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(inString), Key, IV));
        }
        #endregion

        #region public static byte[] Decrypt(...)
        /// <summary>
        /// Decrypts input bytes and returns decrypted data
        /// </summary>
        /// <param name="inBytes">Encrypted Input bytes</param>
        /// <param name="Key">Key to use for decryption process</param>
        /// <param name="IV">Initial vector</param>
        /// <returns>Decrypted data</returns>
        public static byte[] Decrypt(byte[] inBytes, byte[] Key, byte[] IV) {
            using (Aes aes = Aes.Create()) {
                aes.Padding = PaddingMode.Zeros;
                aes.Mode = CipherMode.CBC;
                aes.Key = Key; // Key Size 128
                aes.IV = IV;
                using (MemoryStream memoryStream = new MemoryStream()) {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(Key, IV), CryptoStreamMode.Write)) {
                        cryptoStream.Write(inBytes, 0, inBytes.Length);
                        cryptoStream.Close();
                        return memoryStream.ToArray();
                    }
                }
            }
        }
        #endregion

        #region public static string Decrypt(...)
        /// <summary>
        /// Decrypts input string and returns Decrypted data in raw text
        /// </summary>
        /// <param name="inString">Raw text input</param>
        /// <param name="B64Key">Encryption key in Base64</param>
        /// <param name="B64IV">Initial vector in Base64</param>
        /// <returns>Decrypted raw data string</returns>
        public static string Decrypt(string inString, string B64Key, string B64IV = "") {
            return Encoding.UTF8.GetString(
                Decrypt(
                    Convert.FromBase64String(inString),
                    Convert.FromBase64String(B64Key),
                    B64IV == "" ? new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } : Convert.FromBase64String(B64IV)
                    )
                ).TrimEnd('\0');
        }
        #endregion

        #region public static string Decrypt(...)
        /// <summary>
        /// Decrypts input string and returns Decrypted data in raw text
        /// </summary>
        /// <param name="inString">Base64 string input</param>
        /// <param name="Key">Encryption key in Base64</param>
        /// <param name="IV">Initial vector in byte, if null, default will be used</param>
        /// <returns>Decrypted text string</returns>
        public static string Decrypt(string inString, byte[] Key, byte[] IV = null) {
            if (IV == null)
                IV = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            return Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(inString), Key, IV)).TrimEnd('\0');
        }
        #endregion

    }

}


