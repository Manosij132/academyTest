using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Academy.Shared.Extensions
{
    public static class SecurityExtensions
    {
        private static readonly byte[] key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("Encryption:Key"));
        private static readonly byte[] iv = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("Encryption:Iv"));

        public static string EncryptText(this string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                {
                    using (MemoryStream msEncrypt = new())
                    {
                        using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                            return Convert.ToBase64String(msEncrypt.ToArray());
                        }
                    }
                }
            }
        }

        public static string DecryptText(this string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException(nameof(cipherText));

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                {
                    using (MemoryStream msDecrypt = new(Convert.FromBase64String(cipherText)))
                    {
                        using (CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }
        public static string Decompress(this byte[] compressedData)
        {
            // Decompress the byte array
            byte[] data;
            using (var inputStream = new MemoryStream(compressedData))
            {
                using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                using (var outputStream = new MemoryStream())
                {
                    gzipStream.CopyTo(outputStream);
                    data = outputStream.ToArray();
                }
            }
            return Encoding.UTF8.GetString(data);
        }
        public static byte[] Compress(this string data)
        {
            // Convert string to byte array
            byte[] rawData = Encoding.UTF8.GetBytes(data);

            using (var outputStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    gzipStream.Write(rawData, 0, rawData.Length);
                }
                return outputStream.ToArray();
            }
        }
    }
}
