using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using System.Text.Json;

namespace AzureNamingTool.Services
{
    public class ResourceLocationService
    {
        private static ServiceResponse serviceResponse = new();

        public static async Task<ServiceResponse> GetItems(bool admin = true)
        {
            try
            {
                // Get list of items
                var items = await ConfigurationHelper.GetList<ResourceLocation>();
                if (!admin)
                {
                    serviceResponse.ResponseObject = items.Where(x => x.Enabled == true).OrderBy(x => x.Name).ToList();
                }
                else
                {
                    serviceResponse.ResponseObject = items.OrderBy(x => x.Name).ToList();
                }
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
                var data = await ConfigurationHelper.GetList<ResourceLocation>();
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

        public static async Task<ServiceResponse> PostItem(ResourceLocation item)
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
                var items = await ConfigurationHelper.GetList<ResourceLocation>();

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
                await ConfigurationHelper.WriteList<ResourceLocation>(items);
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
                var items = await ConfigurationHelper.GetList<ResourceLocation>();
                // Get the specified item
                var item = items.Find(x => x.Id == id);
                // Remove the item from the collection
                items.Remove(item);

                // Write items to file
                await ConfigurationHelper.WriteList<ResourceLocation>(items);
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

        public static async Task<ServiceResponse> PostConfig(List<ResourceLocation> items)
        {
            try
            {
                // Get list of items
                var newitems = new List<ResourceLocation>();
                int i = 1;

                // Determine new item id
                foreach (ResourceLocation item in items)
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
                    newitems.Add(item);
                    i += 1;
                }

                // Write items to file
                await ConfigurationHelper.WriteList<ResourceLocation>(newitems);
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

        public static async Task<ServiceResponse> RefreshResourceLocations(bool shortNameReset = false)
        {
            try
            {
                // Get the existing Resource location items
                ServiceResponse serviceResponse;
                serviceResponse = await ResourceLocationService.GetItems();
                List<ResourceLocation> locations = (List<ResourceLocation>)serviceResponse.ResponseObject;
                string url = "https://raw.githubusercontent.com/microsoft/CloudAdoptionFramework/master/ready/AzNamingTool/repository/resourcelocations.json";

                string refreshdata = await GeneralHelper.DownloadString(url);
                if (refreshdata != "")
                {
                    var newlocations = new List<ResourceLocation>();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true
                    };

                    newlocations = JsonSerializer.Deserialize<List<ResourceLocation>>(refreshdata, options);

                    // Loop through the new items
                    // Add any new resource location and update any existing locations
                    foreach (ResourceLocation newlocation in newlocations)
                    {
                        // Check if the existing locations contain the current location
                        int i = locations.FindIndex(x => x.Name == newlocation.Name);
                        if (i > -1)
                        {
                            // Update the Resource location Information
                            ResourceLocation oldlocation = locations[i];
                            newlocation.Enabled = oldlocation.Enabled;
                            
                            if ((!shortNameReset) || (oldlocation.ShortName == ""))
                            {
                                newlocation.ShortName = oldlocation.ShortName;
                            }
                            // Remove the old location
                            locations.RemoveAt(i);
                            // Add the new location
                            locations.Add(newlocation);
                        }
                        else
                        {
                            // Add a new resource location
                            locations.Add(newlocation);
                        }
                    }

                    // Update the settings file
                    serviceResponse = await PostConfig(locations);

                    // Update the repository file
                    await FileSystemHelper.WriteFile("resourcelocations.json", refreshdata, "repository/");

                    // Clear cached data
                    CacheHelper.InvalidateCacheObject("ResourceLocation");

                    // Update the current configuration file version data information
                    await ConfigurationHelper.UpdateConfigurationFileVersion("resourcelocations");
                }
                else
                {
                    AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = "There was a problem refreshing the resource locations configuration." });
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }
    }
}
