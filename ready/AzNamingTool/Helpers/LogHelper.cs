using AzureNamingTool.Models;
using System.Text.Json;

namespace AzureNamingTool.Helpers
{
    public class LogHelper
    {
        /// <summary>
        /// This function gets the gernated names log. 
        /// </summary>
        /// <returns>List of GeneratedNames - List of generated names</returns>
        public static async Task<List<GeneratedName>> GetGeneratedNames()
        {
            List<GeneratedName> lstGeneratedNames = new();
            try
            {
                string data = await FileSystemHelper.ReadFile("generatednames.json");
                var items = new List<GeneratedName>();
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };
                lstGeneratedNames = JsonSerializer.Deserialize<List<GeneratedName>>(data, options).OrderByDescending(x => x.CreatedOn).ToList();
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
            }
            return lstGeneratedNames;
        }

        /// <summary>
        ///  This function logs the generated name. 
        /// </summary>
        /// <param name="lstGeneratedName">GeneratedName - Generated name and components.</param>
        public static async void LogGeneratedName(GeneratedName lstGeneratedName)
        {
            try
            {
                // Log the created name
                var lstGeneratedNames = new List<GeneratedName>();
                lstGeneratedNames = await GetGeneratedNames();

                if (lstGeneratedNames.Count > 0)
                {
                    lstGeneratedName.Id = lstGeneratedNames.Max(x => x.Id) + 1;
                }
                else
                {
                    lstGeneratedName.Id = 1;
                }

                lstGeneratedNames.Add(lstGeneratedName);
                var jsonGeneratedNames = JsonSerializer.Serialize(lstGeneratedNames);
                await FileSystemHelper.WriteFile("generatednames.json", jsonGeneratedNames);
                CacheHelper.InvalidateCacheObject("GeneratedName");
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
            }
        }

        /// <summary>
        /// This function prugres the generated names log. 
        /// </summary>
        /// <returns>void</returns>
        public static async Task PurgeGeneratedNames()
        {
            try
            {
                await FileSystemHelper.WriteFile("generatednames.json", "[]");
                CacheHelper.InvalidateCacheObject("GeneratedName");
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
            }
        }

        /// <summary>
        /// This function returns the Admin log. 
        /// </summary>
        /// <returns>List of AdminLogMessages - List of Adming Log messages.</returns>
        public static async Task<List<AdminLogMessage>> GetAdminLog()
        {
            List<AdminLogMessage> lstAdminLogMessages = new();
            try
            {
                string data = await FileSystemHelper.ReadFile("adminlogmessages.json");
                var items = new List<AdminLogMessage>();
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };
                lstAdminLogMessages = JsonSerializer.Deserialize<List<AdminLogMessage>>(data, options).OrderByDescending(x => x.CreatedOn).ToList();
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
            }
            return lstAdminLogMessages;
        }

        /// <summary>
        /// This function logs the Admin message.
        /// </summary>
        /// <param name="title">string - Message title</param>
        /// <param name="message">string - MEssage body</param>
        public static async void LogAdminMessage(string title, string message)
        {
            try
            {
                AdminLogMessage adminmessage = new()
                {
                    Id = 1,
                    Title = title,
                    Message = message
                };

                // Log the created name
                var lstAdminLogMessages = new List<AdminLogMessage>();
                lstAdminLogMessages = await GetAdminLog();

                if (lstAdminLogMessages.Count > 0)
                {
                    adminmessage.Id = lstAdminLogMessages.Max(x => x.Id) + 1;
                }

                lstAdminLogMessages.Add(adminmessage);
                var jsonAdminLogMessages = JsonSerializer.Serialize(lstAdminLogMessages);
                await FileSystemHelper.WriteFile("adminlogmessages.json", jsonAdminLogMessages);
                CacheHelper.InvalidateCacheObject("AdminLogMessage");
            }
            catch (Exception)
            {
                // No exception is logged due to this function being the function that would complete the action. 
            }
        }

        /// <summary>
        /// This function purges the Admin log. 
        /// </summary>
        /// <returns>void</returns>
        public static async Task PurgeAdminLog()
        {
            try
            {
                await FileSystemHelper.WriteFile("adminlogmessages.json", "[]");
                CacheHelper.InvalidateCacheObject("AdminLogMessage");
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
            }
        }
    }
}
