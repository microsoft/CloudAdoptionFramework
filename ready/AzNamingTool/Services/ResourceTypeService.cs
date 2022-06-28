using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Services
{
    public class ResourceTypeService
    {
        private static ServiceResponse serviceResponse = new();

        public static async Task<ServiceResponse> GetItems()
        {
            try
            {
                // Get list of items
                var items = await GeneralHelper.GetList<ResourceType>();
                serviceResponse.ResponseObject = items.OrderBy(x => x.Resource).ToList(); ;
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                GeneralHelper.LogAdminMessage("ERROR", ex.Message);
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
                var data = await GeneralHelper.GetList<ResourceType>();
                var item = data.Find(x => x.Id == id);
                serviceResponse.ResponseObject = item;
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                GeneralHelper.LogAdminMessage("ERROR", ex.Message);
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }

        public static async Task<ServiceResponse> PostItem(ResourceType item)
        {
            try
            {
                // Make sure the new item short name only contains letters/numbers
                if (!GeneralHelper.CheckAlphanumeric(item.ShortName))
                {
                    serviceResponse.Success = false;
                    serviceResponse.ResponseObject = "Short name must be alphanumeric.";
                    return serviceResponse;
                }

                // Force lowercase on the shortname
                item.ShortName = item.ShortName.ToLower();

                // Get list of items
                var items = await GeneralHelper.GetList<ResourceType>();

                // Set the new id
                if (item.Id == 0)
                {
                    item.Id = items.Count + 1;
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

                    // Put the item at the end
                    items.Add(item);
                }
                else
                {
                    item.Id = 1;
                    items.Add(item);
                }

                // Write items to file
                await GeneralHelper.WriteList<ResourceType>(items);
                serviceResponse.ResponseObject = "Item added!";
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                GeneralHelper.LogAdminMessage("ERROR", ex.Message);
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
                var items = await GeneralHelper.GetList<ResourceType>();
                // Get the specified item
                var item = items.Find(x => x.Id == id);
                // Remove the item from the collection
                items.Remove(item);

                // Write items to file
                await GeneralHelper.WriteList<ResourceType>(items);
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                GeneralHelper.LogAdminMessage("ERROR", ex.Message);
                serviceResponse.ResponseObject = ex;
                serviceResponse.Success = false;
            }
            return serviceResponse;
        }

        public static async Task<ServiceResponse> PostConfig(List<ResourceType> items)
        {
            try
            {
                // Get list of items
                var newitems = new List<ResourceType>();
                int i = 1;

                // Determine new item id
                foreach (ResourceType item in items)
                {

                    // Force lowercase on the shortname
                    item.ShortName = item.ShortName.ToLower();

                    item.Id = i;
                    newitems.Add(item);
                    i += 1;
                }

                // Write items to file
                await GeneralHelper.WriteList<ResourceType>(newitems);
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                GeneralHelper.LogAdminMessage("ERROR", ex.Message);
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }

        public static List<string> GetTypeCategories(List<ResourceType> types)
        {
            List<string> categories = new List<string>();

            foreach (ResourceType type in types)
            {

                string category = type.Resource;
                if (category.Contains("/"))
                {
                    category = category.Substring(0, category.IndexOf("/"));
                }

                if (!categories.Contains(category))
                {
                    categories.Add(category);
                }
            }

            return categories;
        }


        public static List<ResourceType> GetFilteredResourceTypes(List<ResourceType> types, string filter)
        {
            List<ResourceType> filteredtypes = types.Where(x => x.Resource.StartsWith(filter)).ToList();
            return filteredtypes;
        }
    }
}
