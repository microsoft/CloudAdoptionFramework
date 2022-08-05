using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using System.ComponentModel.Design;
using System.Linq;
using System.Text.Json;

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
                LogHelper.LogAdminMessage("ERROR", ex.Message);
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
                LogHelper.LogAdminMessage("ERROR", ex.Message);
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
                LogHelper.LogAdminMessage("ERROR", ex.Message);
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
                LogHelper.LogAdminMessage("ERROR", ex.Message);
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
                LogHelper.LogAdminMessage("ERROR", ex.Message);
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }

        public static List<string> GetTypeCategories(List<ResourceType> types)
        {
            List<string> categories = new();

            foreach (ResourceType type in types)
            {

                string category = type.Resource;
                if (category.Contains('/'))
                {
                    category = category[..category.IndexOf("/")];
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

        public static async Task<bool> RefreshResourceTypes()
        {
            try
            {
                // Get the existing Resource Type items
                ServiceResponse serviceResponse;
                serviceResponse = await ResourceTypeService.GetItems();
                List<ResourceType> types = (List<ResourceType>)serviceResponse.ResponseObject;
                string url = "https://raw.githubusercontent.com/microsoft/CloudAdoptionFramework/master/ready/AzNamingTool/repository/resourcetypes.json";
                
                string refreshdata = await GeneralHelper.DownloadString(url);
                if (refreshdata != "")
                {
                    var newtypes = new List<ResourceType>();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true
                    };

                    newtypes = JsonSerializer.Deserialize<List<ResourceType>>(refreshdata, options);

                    // Loop through the new items
                    // Add any new resource type and update any existing types
                    foreach (ResourceType newtype in newtypes)
                    {
                        // Check if the existing types contain the current type
                        int i = types.FindIndex(x => x.Resource == newtype.Resource);
                        if (i > -1)
                        {
                            ResourceType oldtype = types[i];
                            // Update the Resaource Type Information
                            newtype.Exclude = oldtype.Exclude;
                            newtype.Optional = oldtype.Optional;
                            newtype.ShortName = oldtype.ShortName;
                            // Remove the old type
                            types.RemoveAt(i);
                            // Add the new type
                            types.Add(newtype);
                        }
                        else
                        {
                            // Add a new resource type
                            types.Add(newtype);
                        }
                    }

                    // Update the settings file
                    await PostConfig(types);

                    // Update the repository file
                    File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "repository/resourcetypes.json"), refreshdata);

                    return true;
                }
                else
                {
                    LogHelper.LogAdminMessage("ERROR", "There was a problem refreshing the resource types configuration.");

                }
            }
            catch(Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
            }
            return false;
        }
    }
}
