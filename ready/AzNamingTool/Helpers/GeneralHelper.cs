using AzureNamingTool.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AzureNamingTool.Helpers
{
    public class GeneralHelper
    {
        public static Config GetConfigurationData()
        {
            var config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("settings/appsettings.json")
                    .Build()
                    .Get<Config>();
            return config;
        }

        public static string GetAppSetting(string key)
        {
            string value = null;
            try
            {
                var config = GetConfigurationData();
                value = config.GetType().GetProperty(key).GetValue(config, null).ToString();
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
            }
            return value;
        }

        public static void SetAppSetting(string key, string value)
        {
            try
            {
                var config = GetConfigurationData();
                Type type = config.GetType();
                System.Reflection.PropertyInfo propertyInfo = type.GetProperty(key);
                propertyInfo.SetValue(config, value, null);
                UpdateSettings(config);
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
            }
        }

        //Function to get the Property Value
        public static object GetPropertyValue(object SourceData, string propName)
        {
            try
            {
                return SourceData.GetType().GetProperty(propName).GetValue(SourceData, null);
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
                return null;
            }
        }

        public async static Task<List<T>> GetList<T>()
        {
            try
            {
                String data = String.Empty;
                data = typeof(T).Name switch
                {
                    nameof(ResourceComponent) => await FileSystemHelper.ReadFile("resourcecomponents.json"),
                    nameof(ResourceEnvironment) => await FileSystemHelper.ReadFile("resourceenvironments.json"),
                    nameof(ResourceLocation) => await FileSystemHelper.ReadFile("resourcelocations.json"),
                    nameof(ResourceOrg) => await FileSystemHelper.ReadFile("resourceorgs.json"),
                    nameof(ResourceProjAppSvc) => await FileSystemHelper.ReadFile("resourceprojappsvcs.json"),
                    nameof(ResourceType) => await FileSystemHelper.ReadFile("resourcetypes.json"),
                    nameof(ResourceUnitDept) => await FileSystemHelper.ReadFile("resourceunitdepts.json"),
                    nameof(ResourceFunction) => await FileSystemHelper.ReadFile("resourcefunctions.json"),
                    nameof(ResourceDelimiter) => await FileSystemHelper.ReadFile("resourcedelimiters.json"),
                    _ => "[]",
                };
                var items = new List<T>();
                if (data != "[]")
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true
                    };

                    items = JsonSerializer.Deserialize<List<T>>(data, options);
                }

                return items;
            }
            catch(Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
                throw;
            }
        }

        public async static Task WriteList<T>(List<T> items)
        {
            try
            {
                switch (typeof(T).Name)
                {
                    case nameof(ResourceComponent):
                        await FileSystemHelper.WriteConfiguation(items, "resourcecomponents.json");
                        break;
                    case nameof(ResourceEnvironment):
                        await FileSystemHelper.WriteConfiguation(items, "resourceenvironments.json");
                        break;
                    case nameof(ResourceLocation):
                        await FileSystemHelper.WriteConfiguation(items, "resourcelocations.json");
                        break;
                    case nameof(ResourceOrg):
                        await FileSystemHelper.WriteConfiguation(items, "resourceorgs.json");
                        break;
                    case nameof(ResourceProjAppSvc):
                        await FileSystemHelper.WriteConfiguation(items, "resourceprojappsvcs.json");
                        break;
                    case nameof(ResourceType):
                        await FileSystemHelper.WriteConfiguation(items, "resourcetypes.json");
                        break;
                    case nameof(ResourceUnitDept):
                        await FileSystemHelper.WriteConfiguation(items, "resourceunitdepts.json");
                        break;
                    case nameof(ResourceFunction):
                        await FileSystemHelper.WriteConfiguation(items, "resourcefunctions.json");
                        break;
                    case nameof(ResourceDelimiter):
                        await FileSystemHelper.WriteConfiguation(items, "resourcedelimiters.json");
                        break;
                    default:
                        break;
                }
            }
            catch(Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
                throw;
            }
        }

        public static bool CheckNumeric(string value)
        {
            Regex regx = new ("^[0-9]+$");
            Match match = regx.Match(value);
            return match.Success;
        }

        public static bool CheckAlphanumeric(string value)
        {
            Regex regx = new ("^[a-zA-Z0-9]+$");
            Match match = regx.Match(value);
            return match.Success;
        }

        public static bool ValidatePassword(string text)
        {
            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasMinimum8Chars = new Regex(@".{8,}");

            var isValidated = hasNumber.IsMatch(text) && hasUpperChar.IsMatch(text) && hasMinimum8Chars.IsMatch(text);

            return isValidated;
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
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using MemoryStream memoryStream = new(buffer);
            using CryptoStream cryptoStream = new((Stream)memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new((Stream)cryptoStream);
            return streamReader.ReadToEnd();
        }

        public static void VerifyConfiguration()
        {
            try
            {
                // Get all the files in teh repository folder
                DirectoryInfo dirRepository = new ("repository");
                foreach (FileInfo file in dirRepository.GetFiles())
                {
                    // Check if the file exists in the settings folder
                    if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/" + file.Name)))
                    {
                        // Copy the repository file to the settings folder
                        file.CopyTo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/" + file.Name));
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
            }
        }

        public static void VerifySecurity(StateContainer state)
        {
            try
            {
                var config = GetConfigurationData();
                if (!state.Verified)
                {
                    if (config.SALTKey == "")
                    {
                        // Create a new SALT key 
                        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
                        Random random = new ();
                        var salt = new string(Enumerable.Repeat(chars, 16)
                            .Select(s => s[random.Next(s.Length)]).ToArray());

                        config.SALTKey = salt.ToString();
                        config.APIKey = EncryptString(config.APIKey, salt.ToString());

                        if (config.AdminPassword != "")
                        {
                            config.AdminPassword = EncryptString(config.AdminPassword, config.SALTKey.ToString());
                            state.Password = true;
                        }
                        else
                        {
                            state.Password = false;
                        }
                    }

                    if (config.AdminPassword != "")
                    {
                        state.Password = true;
                    }
                    else
                    {
                        state.Password = false;
                    }
                    UpdateSettings(config);

                }
                state.SetVerified(true);

                // Set the site theme
                state.SetAppTheme(config.AppTheme);
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
            }
        }

        public static void UpdateSettings(Config config)
        {
            var jsonWriteOptions = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            jsonWriteOptions.Converters.Add(new JsonStringEnumConverter());

            var newJson = JsonSerializer.Serialize(config, jsonWriteOptions);

            var appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/appsettings.json");
            File.WriteAllText(appSettingsPath, newJson);
        }

        public static void ResetState(StateContainer state)
        {
            state.SetVerified(false);
            state.SetAdmin(false);
            state.SetPassword(false);
            state.SetAppTheme("bg-default text-black");
        }

        public static bool ValidateShortName(string value, string type)
        {
            bool valid = false;

            switch (type)
            {
                case "ResourceEnvironment":
                    valid = true;
                    break;
                case "ResourceLocation":
                    valid = true;
                    break;
                case "ResourceOrg":
                    if (value.Length < 6)
                    {
                        valid = true;
                    }
                    break;
                case "ResourceProjAppSvc":
                    if (value.Length < 4)
                    {
                        valid = true;
                    }
                    break;
                case "ResourceType":
                    valid = true;
                    break;
                case "ResourceUnitDept":
                    if (value.Length < 4)
                    {
                        valid = true;
                    }
                    break;
                case "ResourceFunction":
                    if (value.Length < 11)
                    {
                        valid = true;
                    }
                    break;
            }

            return valid;
        }

        public static Tuple<bool, string, StringBuilder> ValidateGeneratedName(ResourceType resourceType, string name, string delimiter)
        {
            try
            {
                bool valid = true;
                StringBuilder sbMessage = new();
                // Check regex
                // Validate the name against the resource type regex
                Regex regx = new(resourceType.Regx);
                Match match = regx.Match(name);
                if (!match.Success)
                {
                    // Strip the delimiter in case that is causing the issue
                    name = name.Replace(delimiter, "");

                    Match match2 = regx.Match(name);
                    if (!match2.Success)
                    {
                        sbMessage.Append("Regex failed - Please review the Resource Type Naming Guidelines.");
                        sbMessage.Append(Environment.NewLine);
                        valid = false;
                    }
                    else
                    {
                        sbMessage.Append("The specified delimiter is not allowed for this resource type and has been removed.");
                        sbMessage.Append(Environment.NewLine);
                    }
                }

                // Check min length
                if (int.TryParse(resourceType.LengthMin, out _))
                {
                    if (name.Length < int.Parse(resourceType.LengthMin))
                    {
                        sbMessage.Append("Generated name is less than the minimum length for the selected resource type.");
                        sbMessage.Append(Environment.NewLine);
                        valid = false;
                    }
                }

                // Check max length
                if (int.TryParse(resourceType.LengthMax, out _))
                {
                    if (name.Length > int.Parse(resourceType.LengthMax))
                    {
                        // Strip the delimiter in case that is causing the issue
                        name = name.Replace(delimiter, "");
                        if (name.Length > int.Parse(resourceType.LengthMax))
                        {
                            sbMessage.Append("Generated name is more than the maximum length for the selected resource type.");
                            sbMessage.Append(Environment.NewLine);
                            sbMessage.Append("Please remove any optional components or contact your admin to update the required components for this resource type.");
                            sbMessage.Append(Environment.NewLine);
                            valid = false;
                        }
                        else
                        {
                            sbMessage.Append("Generated name with the selected delimiter is more than the maximum length for the selected resource type. The delimiter has been removed.");
                            sbMessage.Append(Environment.NewLine);
                        }
                    }
                }

                // Check invalid characters
                if (resourceType.InvalidCharacters != "")
                {
                    // Loop through each character
                    foreach (char c in resourceType.InvalidCharacters)
                    {
                        // Check if the name contains the character
                        if (name.Contains(c))
                        {
                            sbMessage.Append("Name cannot contain the following character: " + c);
                            sbMessage.Append(Environment.NewLine);
                            valid = false;
                        }
                    }
                }

                // Check start character
                if (resourceType.InvalidCharactersStart != "")
                {
                    // Loop through each character
                    foreach (char c in resourceType.InvalidCharactersStart)
                    {
                        // Check if the name contains the character
                        if (name.StartsWith(c))
                        {
                            sbMessage.Append("Name cannot start with the following character: " + c);
                            sbMessage.Append(Environment.NewLine);
                            valid = false;
                        }
                    }
                }

                // Check start character
                if (resourceType.InvalidCharactersEnd != "")
                {
                    // Loop through each character
                    foreach (char c in resourceType.InvalidCharactersEnd)
                    {
                        // Check if the name contains the character
                        if (name.EndsWith(c))
                        {
                            sbMessage.Append("Name cannot end with the following character: " + c);
                            sbMessage.Append(Environment.NewLine);
                            valid = false;
                        }
                    }
                }

                // Check consecutive character
                if (resourceType.InvalidCharactersConsecutive != "")
                {
                    // Loop through each character
                    foreach (char c in resourceType.InvalidCharactersConsecutive)
                    {
                        // Check if the name contains the character
                        char current = name[0];
                        for (int i = 1; i < name.Length; i++)
                        {
                            char next = name[i];
                            if ((current == next) && (current == c))
                            {
                                sbMessage.Append("Name cannot contain the following consecutive character: " + next);
                                sbMessage.Append(Environment.NewLine);
                                valid = false;
                                break;
                            }
                            current = next;
                        }
                    }
                }
                return new Tuple<bool, string, StringBuilder>(valid, name, sbMessage);
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
                return new Tuple<bool, string, StringBuilder>(false, name, new StringBuilder("There was a problem validating the name."));
            }
        }

        public static async Task<string> DownloadString(string url)
        {
            string data;
            try
            {
                HttpClient httpClient = new ();
                data = await httpClient.GetStringAsync(url);
            }
            catch(Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
                data = "";
            }
            return data;
        }
    }
}
