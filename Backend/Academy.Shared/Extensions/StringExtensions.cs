using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Academy.Shared.Extensions
{
    public static class StringExtensions
    {
        private static readonly TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
        private const string key = "b14ca5898a4e4133bbce2ea2315a1916";
        public static string Encrypt(this string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using MemoryStream memoryStream = new();
                using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
                using (StreamWriter streamWriter = new(cryptoStream))
                {
                    streamWriter.Write(plainText);
                }

                array = memoryStream.ToArray();
            }

            return Convert.ToBase64String(array);
        }
        public static string Decrypt(this string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new(buffer);
            using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new(cryptoStream);

            return streamReader.ReadToEnd();
        }
        public static List<T> ToList<T>(this string commaSeperatedString)
        {
            if (string.IsNullOrWhiteSpace(commaSeperatedString))
            {
                return [];
            }
            return commaSeperatedString.Split([',']).Select(item => (T)Convert.ChangeType(item, typeof(T))).ToList();
        }
        public static string ToTitleCase(this string str)
        {
            return ti.ToTitleCase(str);
        }
        public static string ToFormattedString(this object value)
        {
            if (value == null || value == DBNull.Value)
                return string.Empty;

            return value switch
            {
                decimal d => d.ToString("F2", CultureInfo.InvariantCulture),
                double d => d.ToString("F2", CultureInfo.InvariantCulture),
                float f => f.ToString("F2", CultureInfo.InvariantCulture),
                DateTime dt => dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), 
                DateTimeOffset dto => dto.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                _ => value.ToString()
            };
        }
    }
}
