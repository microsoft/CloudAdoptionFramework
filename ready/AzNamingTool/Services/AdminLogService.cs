using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Pages;
using System.Text.Json;

namespace AzureNamingTool.Services
{
    public class AdminLogService
    {
        private static ServiceResponse serviceResponse = new();

        /// <summary>
        /// This function returns the Admin log. 
        /// </summary>
        /// <returns>List of AdminLogMessages - List of Adming Log messages.</returns>
        public static async Task<ServiceResponse> GetItems()
        {
            try
            {
                // Get list of items
                var items = await ConfigurationHelper.GetList<AdminLogMessage>();
                serviceResponse.ResponseObject = items.OrderByDescending(x => x.CreatedOn).ToList();
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                await AdminLogService.PostItem(new AdminLogMessage { Title = "Error", Message = ex.Message });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;

        }

        /// <summary>
        /// This function logs the Admin message.
        /// </summary>
        public static async Task<ServiceResponse> PostItem(AdminLogMessage adminlogMessage)
        {
            ServiceResponse serviceReponse = new();
            try
            {
                // Log the created name
                var items = await ConfigurationHelper.GetList<AdminLogMessage>();       
                if (items != null)
                {
                    if (items.Count > 0)
                    {
                        adminlogMessage.Id = items.Max(x => x.Id) + 1;
                    }
                }

                items.Add(adminlogMessage);
                // Write items to file
                await ConfigurationHelper.WriteList<AdminLogMessage>(items);
                serviceReponse.Success = true;
            }
            catch (Exception)
            {
                // No exception is logged due to this function being the function that would complete the action.
                serviceReponse.Success = false;
            }
            return serviceReponse;
        }

        /// <summary>
        /// This function clears the Admin log. 
        /// </summary>
        /// <returns>void</returns>
        public static async Task<ServiceResponse> DeleteAllItems()
        {
            ServiceResponse serviceReponse = new();
            try
            {
                List<AdminLogMessage> lstAdminLogMessages = new();
                await ConfigurationHelper.WriteList<AdminLogMessage>(lstAdminLogMessages);
                serviceReponse.Success = true;
            }
            catch (Exception ex)
            {
                await AdminLogService.PostItem(new AdminLogMessage { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
            }
            return serviceReponse;
        }

        public static async Task<ServiceResponse> PostConfig(List<AdminLogMessage> items)
        {
            try
            {
                // Get list of items
                var newitems = new List<AdminLogMessage>();
                int i = 1;

                // Determine new item id
                foreach (AdminLogMessage item in items)
                {
                    item.Id = i;
                    newitems.Add(item);
                    i += 1;
                }

                // Write items to file
                await ConfigurationHelper.WriteList<AdminLogMessage>(newitems);
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
