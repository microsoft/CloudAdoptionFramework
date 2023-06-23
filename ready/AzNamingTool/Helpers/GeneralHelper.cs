using AzureNamingTool.Models;
using AzureNamingTool.Services;
using System.Security.Cryptography;
using System.Text;

namespace AzureNamingTool.Helpers
{
    public class GeneralHelper
    {
        //Function to get the Property Value
        public static object GetPropertyValue(object SourceData, string propName)
        {
            try
            {
                return SourceData.GetType().GetProperty(propName).GetValue(SourceData, null);
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return null;
            }
        }

        public static string EncryptString(string text, string keyString)
        {
            byte[] iv = new byte[16];
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Key = Encoding.UTF8.GetBytes(keyString);
                aes.IV = iv;
                aes.Padding = PaddingMode.PKCS7;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using MemoryStream memoryStream = new();
                using CryptoStream cryptoStream = new((Stream)memoryStream, encryptor, CryptoStreamMode.Write);
                using (StreamWriter streamWriter = new((Stream)cryptoStream))
                {
                    streamWriter.Write(text);
                }
                array = memoryStream.ToArray();
            }
            return Convert.ToBase64String(array);
        }

        public static string DecryptString(string cipherText, string keyString)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);
            using Aes aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = Encoding.UTF8.GetBytes(keyString);
            aes.IV = iv;
            aes.Padding = PaddingMode.PKCS7;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using MemoryStream memoryStream = new(buffer);
            using CryptoStream cryptoStream = new((Stream)memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new((Stream)cryptoStream);
            return streamReader.ReadToEnd();
        }

        public static bool IsBase64Encoded(string value)
        {
            bool base64encoded = false;
            try
            {
                byte[] byteArray = Convert.FromBase64String(value);
                base64encoded = true;
            }
            catch (FormatException)
            {
                // The string is not base 64. Dismiss the error and return false
            }
            return base64encoded;
        }


        public static async Task<string> DownloadString(string url)
        {
            string data;
            try
            {
                HttpClient httpClient = new();
                data = await httpClient.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                data = "";
            }
            return data;
        }

        public static string NormalizeName(string name, bool lowercase)
        {
            string newname = name.Replace("Resource", "").Replace(" ", "");
            if (lowercase)
            {
                newname = newname.ToLower();
            }
            return newname;
        }

        public static string SetTextEnabledClass(bool enabled)
        {
            if (enabled)
            {
                return "";
            }
            else
            {
                return "disabled-text";
            }
        }
    }
}
