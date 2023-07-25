using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using System.Text.Json;

namespace AzureNamingTool.Services
{
    public class GeneratedNamesService
    {
        private static ServiceResponse serviceResponse = new();

        /// <summary>
        /// This function gets the generated names log. 
        /// </summary>
        /// <returns>List of GeneratedNames - List of generated names</returns>
        public static async Task<ServiceResponse> GetItems() 
        {
            List<GeneratedName> lstGeneratedNames = new();
            try
            {
                // Get list of items
                var items = await ConfigurationHelper.GetList<GeneratedName>();
                serviceResponse.ResponseObject = items.OrderByDescending(x => x.CreatedOn).ToList();
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                await AdminLogService.PostItem(new AdminLogMessage{ Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
            }
            return serviceResponse;
        }

        public static async Task<ServiceResponse> GetItem(int id)
        {
            try
            {
                // Get list of items
                var data = await ConfigurationHelper.GetList<GeneratedName>();
                var item = data.Find(x => x.Id == id);
                serviceResponse.ResponseObject = item;
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }

        /// <summary>
        ///  This function logs the generated name. 
        /// </summary>
        /// <param name="lstGeneratedName">GeneratedName - Generated name and components.</param>
        public static async Task<ServiceResponse> PostItem(GeneratedName generatedName)
        {
            ServiceResponse serviceReponse = new();
            try
            {
                /// Get the previously generated names
                var items = await ConfigurationHelper.GetList<GeneratedName>();
                if ((items != null) && (items.Count > 0))
                {
                    generatedName.Id = items.Max(x => x.Id) + 1;
                }
                else
                {
                    generatedName.Id = 1;
                }
                
                items.Add(generatedName);

                // Write items to file
                ConfigurationHelper.WriteList<GeneratedName>(items);

                CacheHelper.InvalidateCacheObject("generatednames.json");

                serviceReponse.Success = true;
            }
            catch (Exception ex)
            {
                await AdminLogService.PostItem(new AdminLogMessage { Title = "ERROR", Message = ex.Message });
                serviceReponse.Success = false;
            }
            return serviceReponse;
        }

        public static async Task<ServiceResponse> DeleteItem(int id)
        {
            try
            {
                // Get list of items
                var items = await ConfigurationHelper.GetList<GeneratedName>();
                // Get the specified item
                var item = items.Find(x => x.Id == id);
                // Remove the item from the collection
                items.Remove(item);

                // Write items to file
                await ConfigurationHelper.WriteList<GeneratedName>(items);
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.ResponseObject = ex;
                serviceResponse.Success = false;
            }
            return serviceResponse;
        }

        /// <summary>
        /// This function clears the generated names log. 
        /// </summary>
        /// <returns>void</returns>
        public static async Task<ServiceResponse> DeleteAllItems()
        {
            ServiceResponse serviceReponse = new();
            try
            {
                List<GeneratedName> items = new List<GeneratedName>();
                await ConfigurationHelper.WriteList<GeneratedName>(items);
                serviceReponse.Success = true;
            }
            catch (Exception ex)
            {
                await AdminLogService.PostItem(new AdminLogMessage { Title = "Error", Message = ex.Message });
                serviceResponse.Success = false;
            }
            return serviceReponse;
        }

        public static async Task<ServiceResponse> PostConfig(List<GeneratedName> items)
        {
            try
            {
                // Get list of items
                var newitems = new List<GeneratedName>();
                int i = 1;

                // Determine new item id
                foreach (GeneratedName item in items)
                {
                    item.Id = i;
                    newitems.Add(item);
                    i += 1;
                }

                // Write items to file
                await ConfigurationHelper.WriteList<GeneratedName>(newitems);
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.ResponseObject = ex;
                serviceResponse.Success = false;
            }
            return serviceResponse;
        }
    }
}