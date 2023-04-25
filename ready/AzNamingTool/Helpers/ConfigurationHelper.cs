using AzureNamingTool.Models;
using System.Text.Json.Serialization;
using System.Text.Json;
using AzureNamingTool.Services;
using System.Xml.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Runtime.Caching;
using System.Reflection;
using System.Net.Http;
using System.Text;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System;
using Blazored.Toast.Services;

namespace AzureNamingTool.Helpers
{
    public class ConfigurationHelper
    {
        public static SiteConfiguration GetConfigurationData()
        {
            var config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("settings/appsettings.json")
                    .Build()
                    .Get<SiteConfiguration>();
            return config;
        }

        public static string GetAppSetting(string key, bool decrypt = false)
        {
            string value = null;
            try
            {
                // Check if the data is cached
                var data = CacheHelper.GetCacheObject(key);
                if (data == null)
                {
                    var config = GetConfigurationData();

                    if (config.GetType().GetProperty(key) != null)
                    {
                        value = config.GetType().GetProperty(key).GetValue(config, null).ToString();

                        if ((decrypt) && (value != ""))
                        {
                            value = GeneralHelper.DecryptString(value, config.SALTKey);
                        }

                        // Set the result to cache
                        CacheHelper.SetCacheObject(key, value);
                    }
                }
                else
                {
                    value = data.ToString();
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
            return value;
        }

        public static void SetAppSetting(string key, string value, bool encrypt = false)
        {
            try
            {
                var config = GetConfigurationData();
                string valueoriginal = value;
                if (encrypt)
                {
                    value = GeneralHelper.EncryptString(value, config.SALTKey);
                }
                Type type = config.GetType();
                System.Reflection.PropertyInfo propertyInfo = type.GetProperty(key);
                propertyInfo.SetValue(config, value, null);
                UpdateSettings(config);
                // Save the original value to the cache
                CacheHelper.SetCacheObject(key, valueoriginal);
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
        }

        public static void VerifyConfiguration()
        {
            try
            {
                // Get all the files in teh repository folder
                DirectoryInfo repositoryDir = new("repository");
                foreach (FileInfo file in repositoryDir.GetFiles())
                {
                    // Check if the file exists in the settings folder
                    if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/" + file.Name)))
                    {
                        // Copy the repository file to the settings folder
                        file.CopyTo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/" + file.Name));
                    }
                }

                // Migrate old data to new files, if needed
                // Check if the admin log file exists in the settings folder and the adminmessages does not
                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/adminlog.json")) && !File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/adminlogmessages.json")))
                {
                    // Migrate the data
                    FileSystemHelper.MigrateDataToFile("adminlog.json", "settings/", "adminlogmessages.json", "settings/", true);
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
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
                        Random random = new();
                        var salt = new string(Enumerable.Repeat(chars, 16)
                            .Select(s => s[random.Next(s.Length)]).ToArray());

                        config.SALTKey = salt.ToString();
                        config.APIKey = GeneralHelper.EncryptString(config.APIKey, salt.ToString());

                        if (config.AdminPassword != "")
                        {
                            config.AdminPassword = GeneralHelper.EncryptString(config.AdminPassword, config.SALTKey.ToString());
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
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
        }

        public static bool VerifyConnectivity()
        {
            bool pingsuccessful = false;
            bool result = false;
            try
            {
                // Check if the data is cached
                var data = CacheHelper.GetCacheObject("isconnected");
                if (data == null)
                {
                    // Check if the connectivity check is enabled
                    if (Convert.ToBoolean(ConfigurationHelper.GetAppSetting("ConnectivityCheckEnabled")))
                    {
                        // Atempt to ping a url first
                        Ping ping = new();
                        String host = "github.com";
                        byte[] buffer = new byte[32];
                        int timeout = 1000;
                        PingOptions pingOptions = new();
                        PingReply reply = ping.Send(host, timeout, buffer, pingOptions);
                        if (reply.Status == IPStatus.Success)
                        {
                            pingsuccessful = true;
                            result = true;
                        }

                        // If ping is not successful, attempt to download a file
                        if (!pingsuccessful)
                        {
                            // Atempt to download a file
                            var request = (HttpWebRequest)WebRequest.Create("https://github.com/aznamingtool/AzureNamingTool/blob/main/connectiontest.png");
                            request.KeepAlive = false;
                            request.Timeout = 1500;
                            using (var response = (HttpWebResponse)request.GetResponse())
                            {
                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    result = true;
                                }
                                else
                                {
                                    AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = "Connectivity Check Failed:" + response.StatusDescription });
                                }
                            }
                        }
                    }
                    else
                    {
                        result = true;
                    }
                }
                else
                {
                    result = (bool)data;
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }

            // Set the result to cache
            CacheHelper.SetCacheObject("isconnected", result);
            return result;
        }

        public async static Task<List<T>> GetList<T>()
        {
            var items = new List<T>();
            try
            {
                // Check if the data is cached
                String data = (string)CacheHelper.GetCacheObject(typeof(T).Name);
                // Load the data from the file system.
                if ((data == null) || (data == ""))
                {
                    data = typeof(T).Name switch
                    {
                        nameof(ResourceComponent) => await FileSystemHelper.ReadFile("resourcecomponents.json"),
                        nameof(ResourceEnvironment) => await FileSystemHelper.ReadFile("resourceenvironments.json"),
                        nameof(Models.ResourceLocation) => await FileSystemHelper.ReadFile("resourcelocations.json"),
                        nameof(ResourceOrg) => await FileSystemHelper.ReadFile("resourceorgs.json"),
                        nameof(ResourceProjAppSvc) => await FileSystemHelper.ReadFile("resourceprojappsvcs.json"),
                        nameof(Models.ResourceType) => await FileSystemHelper.ReadFile("resourcetypes.json"),
                        nameof(ResourceUnitDept) => await FileSystemHelper.ReadFile("resourceunitdepts.json"),
                        nameof(ResourceFunction) => await FileSystemHelper.ReadFile("resourcefunctions.json"),
                        nameof(ResourceDelimiter) => await FileSystemHelper.ReadFile("resourcedelimiters.json"),
                        nameof(CustomComponent) => await FileSystemHelper.ReadFile("customcomponents.json"),
                        nameof(AdminLogMessage) => await FileSystemHelper.ReadFile("adminlogmessages.json"),
                        nameof(GeneratedName) => await FileSystemHelper.ReadFile("generatednames.json"),
                        _ => "[]",
                    };
                    CacheHelper.SetCacheObject(typeof(T).Name, data);
                }

                if (data != "[]")
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true
                    };

                    items = JsonSerializer.Deserialize<List<T>>(data, options);
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
            return items;
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
                    case nameof(Models.ResourceLocation):
                        await FileSystemHelper.WriteConfiguation(items, "resourcelocations.json");
                        break;
                    case nameof(ResourceOrg):
                        await FileSystemHelper.WriteConfiguation(items, "resourceorgs.json");
                        break;
                    case nameof(ResourceProjAppSvc):
                        await FileSystemHelper.WriteConfiguation(items, "resourceprojappsvcs.json");
                        break;
                    case nameof(Models.ResourceType):
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
                    case nameof(CustomComponent):
                        await FileSystemHelper.WriteConfiguation(items, "customcomponents.json");
                        break;
                    case nameof(AdminLogMessage):
                        await FileSystemHelper.WriteConfiguation(items, "adminlogmessages.json");
                        break;
                    case nameof(GeneratedName):
                        await FileSystemHelper.WriteConfiguation(items, "generatednames.json");
                        break;
                    default:
                        break;
                }

                String data = String.Empty;
                data = typeof(T).Name switch
                {
                    nameof(ResourceComponent) => await FileSystemHelper.ReadFile("resourcecomponents.json"),
                    nameof(ResourceEnvironment) => await FileSystemHelper.ReadFile("resourceenvironments.json"),
                    nameof(Models.ResourceLocation) => await FileSystemHelper.ReadFile("resourcelocations.json"),
                    nameof(ResourceOrg) => await FileSystemHelper.ReadFile("resourceorgs.json"),
                    nameof(ResourceProjAppSvc) => await FileSystemHelper.ReadFile("resourceprojappsvcs.json"),
                    nameof(Models.ResourceType) => await FileSystemHelper.ReadFile("resourcetypes.json"),
                    nameof(ResourceUnitDept) => await FileSystemHelper.ReadFile("resourceunitdepts.json"),
                    nameof(ResourceFunction) => await FileSystemHelper.ReadFile("resourcefunctions.json"),
                    nameof(ResourceDelimiter) => await FileSystemHelper.ReadFile("resourcedelimiters.json"),
                    nameof(CustomComponent) => await FileSystemHelper.ReadFile("customcomponents.json"),
                    nameof(AdminLogMessage) => await FileSystemHelper.ReadFile("adminlogmessages.json"),
                    nameof(GeneratedName) => await FileSystemHelper.ReadFile("generatednames.json"),
                    _ => "[]",
                };

                // Update the cache with the latest data
                CacheHelper.SetCacheObject(typeof(T).Name, data);
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
        }

        public static async void UpdateSettings(SiteConfiguration config)
        {
            var jsonWriteOptions = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            jsonWriteOptions.Converters.Add(new JsonStringEnumConverter());

            var newJson = JsonSerializer.Serialize(config, jsonWriteOptions);

            var appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/appsettings.json");
            await FileSystemHelper.WriteFile("appsettings.json", newJson);
        }

        public static async Task<string> GetOfficalConfigurationFileVersionData()
        {
            string versiondata = null;
            try
            {
                versiondata = await GeneralHelper.DownloadString("https://raw.githubusercontent.com/microsoft/CloudAdoptionFramework/master/ready/AzNamingTool/configurationfileversions.json");
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
            return versiondata;
        }

        public static async Task<string> GetCurrentConfigFileVersionData()
        {
            string versiondatajson = null;
            try
            {
                versiondatajson = await FileSystemHelper.ReadFile("configurationfileversions.json");
                // Check if the user has any version data. This value will be '[]' if not.
                if (versiondatajson == "[]")
                {
                    // Create new version data with default values in /settings file
                    ConfigurationFileVersionData? versiondata = new();
                    await FileSystemHelper.WriteFile("configurationfileversions.json", JsonSerializer.Serialize(versiondata), "settings/");
                    versiondatajson = JsonSerializer.Serialize(versiondata);
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
            return versiondatajson;
        }

        public static async Task<List<string>> VerifyConfigurationFileVersionData()
        {
            List<string> versiondata = new();
            try
            {
                // Get the official version from GitHub
                ConfigurationFileVersionData? officialversiondata = new();
                var officialdatajson = await GetOfficalConfigurationFileVersionData();

                // Get the current version
                ConfigurationFileVersionData? currentversiondata = new();
                var currentdatajson = await GetCurrentConfigFileVersionData();

                // Determine if the version data is different
                if ((officialdatajson != null) && (currentdatajson != null))
                {
                    officialversiondata = JsonSerializer.Deserialize<ConfigurationFileVersionData>(officialdatajson);
                    currentversiondata = JsonSerializer.Deserialize<ConfigurationFileVersionData>(currentdatajson);

                    // Compare the versions
                    // Resource Types
                    if (officialversiondata.resourcetypes != currentversiondata.resourcetypes)
                    {
                        versiondata.Add("<h5>Resource Types</h5><hr /><div>Your resource types configuration is out of date!<br /><br />It is recommended that you refresh your resource types to the latest configuration.<br /><br /><strong>To Refresh:</strong><ul><li>Expand the <strong>Types</strong> section</li><li>Expand the <strong>Configuration</strong> section</li><li>Select the <strong>Refresh</strong> option</li></ul></div><br />");
                    }

                    // Resource Locations
                    if (officialversiondata.resourcelocations != currentversiondata.resourcelocations)
                    {
                        versiondata.Add("<h5>Resource Locations</h5><hr /><div>Your resource locations configuration is out of date!<br /><br />It is recommended that you refresh your resource locations to the latest configuration.<br /><br /><strong>To Refresh:</strong><ul><li>Expand the <strong>Locations</strong> section</li><li>Expand the <strong>Configuration</strong> section</li><li>Select the <strong>Refresh</strong> option</li></ul></div><br />");
                    }
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
            return versiondata;
        }

        public static async Task UpdateConfigurationFileVersion(string fileName)
        {
            if (VerifyConnectivity())
            {
                try
                {
                    // Get the official version from GitHub
                    ConfigurationFileVersionData? officialversiondata = new();
                    var officialdatajson = await GetOfficalConfigurationFileVersionData();

                    // Get the current version
                    ConfigurationFileVersionData? currentversiondata = new();
                    var currentdatajson = await GetCurrentConfigFileVersionData();

                    // Determine if the version data is different
                    if ((officialdatajson != null) && (currentdatajson != null))
                    {
                        officialversiondata = JsonSerializer.Deserialize<ConfigurationFileVersionData>(officialdatajson);
                        currentversiondata = JsonSerializer.Deserialize<ConfigurationFileVersionData>(currentdatajson);

                        switch (fileName)
                        {
                            case "resourcetypes":
                                currentversiondata.resourcetypes = officialversiondata.resourcetypes;
                                break;
                            case "resourcelocations":
                                currentversiondata.resourcelocations = officialversiondata.resourcelocations;
                                break;
                        }
                        //  Update the current configuration file version data
                        await FileSystemHelper.WriteFile("configurationfileversions.json", JsonSerializer.Serialize(currentversiondata), "settings/");
                    }
                }
                catch (Exception ex)
                {
                    AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                }
            }
        }

        public static bool ResetSiteConfiguration()
        {
            bool result = false;
            try
            {
                // Get all the files in teh repository folder
                DirectoryInfo repositoryDir = new("repository");
                // Filter out teh appsettings.json to retain admin credentials
                foreach (FileInfo file in repositoryDir.GetFiles().Where(x => x.Name != "appsettings.json"))
                {
                    // Copy the repository file to the settings folder
                    file.CopyTo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/" + file.Name), true);
                }

                // Clear the cache
                ObjectCache memoryCache = MemoryCache.Default;
                List<string> cacheKeys = memoryCache.Select(kvp => kvp.Key).ToList();
                foreach (string cacheKey in cacheKeys)
                {
                    memoryCache.Remove(cacheKey);
                }
                result = true;
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
            return result;
        }

        public static void ResetState(StateContainer state)
        {
            state.SetVerified(false);
            state.SetAdmin(false);
            state.SetPassword(false);
            state.SetAppTheme("bg-default text-black");
        }

        public static async Task<string> GetToolVersion()
        {
            try
            {
                var response = await GeneralHelper.DownloadString("https://raw.githubusercontent.com/microsoft/CloudAdoptionFramework/master/ready/AzNamingTool/AzureNamingTool.csproj");
                XDocument xdoc = XDocument.Parse(response);
                string result = xdoc
                    .Descendants("PropertyGroup")
                    .Descendants("Version")
                    .First()
                    .Value;
                return result;
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return null;
            }
        }

        public static async Task<string> GetVersionAlert(bool forceDisplay = false)
        {
            string alert = "";
            try
            {
                VersionAlert versionalert = new();
                bool dismissed = false;
                string appversion = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

                // Check if version alert has been dismissed
                var dismissedalerts = GetAppSetting("DismissedAlerts").Split(',');
                if (dismissedalerts != null)
                {
                    if (dismissedalerts.Contains(appversion))
                    {
                        dismissed = true;
                    }
                }

                if ((!dismissed) || (forceDisplay))
                {
                    // Check if the data is cached
                    var cacheddata = CacheHelper.GetCacheObject("versionalert-" + appversion);
                    if (cacheddata == null)
                    {
                        // Get the alert 
                        List<VersionAlert> versionAlerts = new();
                        string data = await FileSystemHelper.ReadFile("versionalerts.json", "");
                        if (data != null)
                        {
                            var items = new List<VersionAlert>();
                            var options = new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                PropertyNameCaseInsensitive = true
                            };
                            items = JsonSerializer.Deserialize<List<VersionAlert>>(data, options).ToList();
                            versionalert = items.Where(x => x.Version == appversion).FirstOrDefault();

                            if (versionalert != null)
                            {
                                alert = versionalert.Alert;
                                // Set the result to cache
                                CacheHelper.SetCacheObject("versionalert-" + appversion, versionalert.Alert);
                            }
                        }
                    }
                    else
                    {
                        alert = (string)cacheddata;
                    }
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
            return alert;
        }

        public static void DismissVersionAlert()
        {
            try
            {
                string appversion = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                List<string> dismissedalerts = new(GetAppSetting("DismissedAlerts").Split(','));
                if (!dismissedalerts.Contains(appversion))
                {
                    if (string.Join(",", dismissedalerts) == "")
                    {
                        dismissedalerts.Clear();
                    }
                    dismissedalerts.Add(appversion);
                }
                SetAppSetting("DismissedAlerts", string.Join(",", dismissedalerts));
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
        }

        public static bool VerifyDuplicateNamesAllowed()
        {
            bool result = false;
            try
            {
                // Check if the data is cached
                var cacheddata = CacheHelper.GetCacheObject("duplicatenamesallowed");
                if (cacheddata == null)
                {
                    // Check if version alert has been dismissed
                    var allowed = GetAppSetting("DuplicateNamesAllowed");
                    result = Convert.ToBoolean(allowed);
                    // Set the result to cache
                    CacheHelper.SetCacheObject("duplicatenamesallowed", result);
                }
                else
                {
                    result = Convert.ToBoolean(cacheddata);
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
            return result;
        }

        public static async Task<bool> PostToGenerationWebhook(string URL, GeneratedName generatedName)
        {
            bool result = false;
            try
            {
                HttpClient httpClient = new()
                {
                    BaseAddress = new Uri(URL)
                };
                HttpResponseMessage response = await httpClient.PostAsJsonAsync("", generatedName);
                if (response.IsSuccessStatusCode)
                {
                    result = true;
                    AdminLogService.PostItem(new AdminLogMessage() { Title = "INFORMATION", Message = "Generated Name (" + generatedName.ResourceName + ") successfully posted to webhook!" });
                }
                else
                {
                    AdminLogService.PostItem(new AdminLogMessage() { Title = "INFORMATION", Message = "Generated Name (" + generatedName.ResourceName + ") not successfully posted to webhook! " + response.ReasonPhrase });
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "INFORMATION", Message = "Generated Name (" + generatedName.ResourceName + ") not successfully posted to webhook! " + ex.Message });
            }
            return result;
        }
        public static async Task<string> GetProgramSetting(string programSetting)
        {
            string result = String.Empty;
            try
            {
                string data = (string)CacheHelper.GetCacheObject(programSetting);
                if (String.IsNullOrEmpty(data))
                {
                    var response = await GeneralHelper.DownloadString("https://raw.githubusercontent.com/aznamingtool/AzureNamingTool/main/programsettings.json");
                    var setting = JsonDocument.Parse(response);
                    result = setting.RootElement.GetProperty(programSetting).ToString();
                    CacheHelper.SetCacheObject(programSetting, result);
                }
                else
                {
                    result = data;
                }
                return result;
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
            return result;
        }
    }
}
