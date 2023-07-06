using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Services
{
    public class AdminUserService
    {
        private static ServiceResponse serviceResponse = new();

        public static async Task<ServiceResponse> GetItems()
        {
            try
            {
                // Get list of items
                var items = await ConfigurationHelper.GetList<AdminUser>();
                serviceResponse.ResponseObject = items.OrderBy(x => x.Name).ToList();
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

        public static async Task<ServiceResponse> GetItem(string name)
        {
            try
            {
                // Get list of items
                var data = await ConfigurationHelper.GetList<AdminUser>();
                var item = data.Find(x => x.Name == name);
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

        public static async Task<ServiceResponse> PostItem(AdminUser item)
        {
            try
            {
                // Get list of items
                var items = await ConfigurationHelper.GetList<AdminUser>();

                // Set the new id
                if (item.Id == 0)
                {
                    if (items.Count > 0)
                    {
                        item.Id = items.Max(t => t.Id) + 1;
                    }
                    else
                    {
                        item.Id = 1;
                    }
                }

                int position = 1;
                items = items.OrderBy(x => x.Name).ToList();

                // Determine new item id
                if (items.Count > 0)
                {
                    // Check if the item already exists
                    if (items.Exists(x => x.Id == item.Id))
                    {
                        // Remove the updated item from the list
                        var existingitem = items.Find(x => x.Id == item.Id);
                        int index = items.IndexOf(existingitem);
                        items.RemoveAt(index);
                    }


                    // Check for the new sort order
                    if (items.Exists(x => x.Id == item.Id))
                    {
                        // Remove the updated item from the list
                        items.Insert(items.IndexOf(items.FirstOrDefault(x => x.Id == item.Id)), item);
                    }
                    else
                    {
                        // Put the item at the end
                        items.Add(item);
                    }
                }
                else
                {
                    item.Id = 1;
                    items.Add(item);
                }

                position = 1;

                // Write items to file
                await ConfigurationHelper.WriteList<AdminUser>(items);
                serviceResponse.ResponseObject = "Item added!";
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

        public static async Task<ServiceResponse> DeleteItem(int id)
        {
            try
            {
                // Get list of items
                var items = await ConfigurationHelper.GetList<AdminUser>();
                // Get the specified item
                var item = items.Find(x => x.Id == id);
                // Remove the item from the collection
                items.Remove(item);

                // Write items to file
                await ConfigurationHelper.WriteList<AdminUser>(items);
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

        public static async Task<ServiceResponse> PostConfig(List<AdminUser> items)
        {
            try
            {
                // Get list of items
                var newitems = new List<AdminUser>();
                int i = 1;

                // Determine new item id
                foreach (AdminUser item in items)
                {
                    item.Id = i;
                    newitems.Add(item);
                    i += 1;
                }

                // Write items to file
                await ConfigurationHelper.WriteList<AdminUser>(newitems);
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