using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace AzureNamingTool.Helpers
{
    public class FileSystemHelper
    {
        public static async Task<string> ReadFile(string fileName)
        {
            await CheckFile(fileName);
            string data = await File.ReadAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/" + fileName));
            return data;
        }

        public static async Task WriteFile(string fileName, string content)
        {
            await CheckFile(fileName);
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/" + fileName), content);
        }

        public static async Task CheckFile(string fileName)
        {
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/" + fileName)))
            {
                var file = File.Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/" + fileName));
                file.Close();

                for (int numTries = 0; numTries < 10; numTries++)
                {
                    try
                    {
                        await File.WriteAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/" + fileName), "[]");
                        return;
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(50);
                    }
                }
            }
        }

        public static async Task<object> WriteConfiguation(object configdata, string configFileName)
        {
            try
            {

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                await FileSystemHelper.WriteFile(configFileName, JsonSerializer.Serialize(configdata, options));
                return "Config updated.";
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
                return ex;
            }
        }

        public static bool ResetConfiguration(string filename)
        {
            bool result = false;
            try
            {
                // Get all the files in teh repository folder
                DirectoryInfo dirRepository = new("repository");
                foreach (FileInfo file in dirRepository.GetFiles())
                {
                    if(file.Name == filename)
                    { 
                        // Copy the repository file to the settings folder
                        file.CopyTo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/" + file.Name), true);
                        result = true;
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
            }
            return result;
        }
    }
}
