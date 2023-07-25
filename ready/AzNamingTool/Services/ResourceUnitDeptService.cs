using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Services
{
    public class ResourceUnitDeptService
    {
        private static ServiceResponse serviceResponse = new();

        public static async Task<ServiceResponse> GetItems()
        {
            try
            {
                // Get list of items
                var items = await ConfigurationHelper.GetList<ResourceUnitDept>();
                serviceResponse.ResponseObject = items.OrderBy(x => x.SortOrder).ToList();
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

        public static async Task<ServiceResponse> GetItem(int id)
        {
            try
            {
                // Get list of items
                var data = await ConfigurationHelper.GetList<ResourceUnitDept>();
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

        public static async Task<ServiceResponse> PostItem(ResourceUnitDept item)
        {
            try
            {
                // Make sure the new item short name only contains letters/numbers
                if (!ValidationHelper.CheckAlphanumeric(item.ShortName))
                {
                    serviceResponse.Success = false;
                    serviceResponse.ResponseObject = "Short name must be alphanumeric.";
                    return serviceResponse;
                }

                // Force lowercase on the shortname
                item.ShortName = item.ShortName.ToLower();

                // Get list of items
                var items = await ConfigurationHelper.GetList<ResourceUnitDept>();

                // Set the new id
                if (item.Id == 0)
                {
                    item.Id = items.Count + 1;
                }

                int position = 1;
                items = items.OrderBy(x => x.SortOrder).ToList();

                if (item.SortOrder == 0)
                {
                    item.Id = items.Max(t => t.Id) + 1;
                }

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

                    // Reset the sort order of the list
                    foreach (ResourceUnitDept thisitem in items.OrderBy(x => x.SortOrder).ToList())
                    {
                        thisitem.SortOrder = position;
                        position += 1;
                    }

                    // Check for the new sort order
                    if (items.Exists(x => x.SortOrder == item.SortOrder))
                    {
                        // Remove the updated item from the list
                        items.Insert(items.IndexOf(items.FirstOrDefault(x => x.SortOrder == item.SortOrder)), item);
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
                    item.SortOrder = 1;
                    items.Add(item);
                }

                position = 1;
                foreach (ResourceUnitDept thisitem in items.OrderBy(x => x.SortOrder).ToList())
                {
                    thisitem.SortOrder = position;
                    position += 1;
                }

                // Write items to file
                await ConfigurationHelper.WriteList<ResourceUnitDept>(items);
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
                var items = await ConfigurationHelper.GetList<ResourceUnitDept>();
                // Get the specified item
                var item = items.Find(x => x.Id == id);
                // Remove the item from the collection
                items.Remove(item);

                // Update all the sort order values to reflect the removal
                int position = 1;
                foreach (ResourceUnitDept thisitem in items.OrderBy(x => x.SortOrder).ToList())
                {
                    thisitem.SortOrder = position;
                    position += 1;
                }

                // Write items to file
                await ConfigurationHelper.WriteList<ResourceUnitDept>(items);
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

        public static async Task<ServiceResponse> PostConfig(List<ResourceUnitDept> items)
        {
            try
            {
                // Get list of items
                var newitems = new List<ResourceUnitDept>();
                int i = 1;

                // Determine new item id
                foreach (ResourceUnitDept item in items)
                {
                    // Make sure the new item short name only contains letters/numbers
                    if (!ValidationHelper.CheckAlphanumeric(item.ShortName))
                    {
                        serviceResponse.Success = false;
                        serviceResponse.ResponseObject = "Short name must be alphanumeric.";
                        return serviceResponse;
                    }

                    // Force lowercase on the shortname
                    item.ShortName = item.ShortName.ToLower();

                    item.Id = i;
                    item.SortOrder = i;
                    newitems.Add(item);
                    i += 1;
                }

                // Write items to file
                await ConfigurationHelper.WriteList<ResourceUnitDept>(newitems);
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