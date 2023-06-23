using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using System.Net.WebSockets;
using System.Security.AccessControl;

namespace AzureNamingTool.Services
{
    public class ResourceComponentService
    {
        private static ServiceResponse serviceResponse = new();

        public static async Task<ServiceResponse> GetItems(bool admin)
        {
            try
            {
                var items = await ConfigurationHelper.GetList<ResourceComponent>();
                if (!admin)
                {
                    serviceResponse.ResponseObject = items.Where(x => x.Enabled == true).OrderBy(y => y.SortOrder).ToList();
                }
                else
                {
                    serviceResponse.ResponseObject = items.OrderBy(y => y.SortOrder).OrderByDescending(y => y.Enabled).ToList();
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
                var data = await ConfigurationHelper.GetList<ResourceComponent>();
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

        public static async Task<ServiceResponse> PostItem(ResourceComponent item)
        {
            try
            {
                // Get list of items
                var items = await ConfigurationHelper.GetList<ResourceComponent>();

                // Set the new id
                if (item.Id == 0)
                {
                    item.Id = items.Count + 1;
                }

                int position = 1;
                items = items.OrderBy(x => x.SortOrder).ToList();

                if (item.SortOrder == 0)
                {
                    item.SortOrder = items.Count + 1;
                }

                // Determine new item id
                if (items.Count > 0)
                {
                    // Check if the item already exists
                    if (items.Exists(x => x.Name == item.Name))
                    {
                        // Remove the updated item from the list
                        var existingitem = items.Find(x => (x.Name == item.Name));
                        int index = items.IndexOf(existingitem);
                        items.RemoveAt(index);
                    }

                    // Reset the sort order of the list
                    foreach (ResourceComponent thisitem in items.OrderBy(x => x.SortOrder).OrderByDescending(x => x.Enabled).ToList())
                    {
                        thisitem.SortOrder = position;
                        position += 1;
                    }

                    // Check for the new sort order
                    if (items.Exists(x => x.SortOrder == item.SortOrder))
                    {
                        // Insert the new item
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
                foreach (ResourceComponent thisitem in items.OrderBy(x => x.SortOrder).OrderByDescending(x => x.Enabled).ToList())
                {
                    thisitem.SortOrder = position;
                    thisitem.Id = position;
                    position += 1;
                }

                // Write items to file
                await ConfigurationHelper.WriteList<ResourceComponent>(items);
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

        public static async Task<ServiceResponse> DeleteItem(int id)
        {
            try
            {
                // Get list of items
                var items = await ConfigurationHelper.GetList<ResourceComponent>();
                // Get the specified item
                var item = items.Find(x => x.Id == id);

                // Delete any resource type settings for the component
                List<string> currentvalues = new();
                serviceResponse = await ResourceTypeService.GetItems();
                List<Models.ResourceType> resourceTypes = (List<Models.ResourceType>)serviceResponse.ResponseObject;
                foreach (Models.ResourceType currenttype in resourceTypes)
                {
                    currentvalues = new List<string>(currenttype.Optional.Split(','));
                    if (currentvalues.Contains(GeneralHelper.NormalizeName(item.Name, false)))
                    {
                        currentvalues.Remove(GeneralHelper.NormalizeName(item.Name, false));
                        currenttype.Optional = String.Join(",", currentvalues.ToArray());
                    }

                    currentvalues = new List<string>(currenttype.Exclude.Split(','));
                    if (currentvalues.Contains(GeneralHelper.NormalizeName(item.Name, false)))
                    {
                        currentvalues.Remove(GeneralHelper.NormalizeName(item.Name, false));
                        currenttype.Exclude = String.Join(",", currentvalues.ToArray());
                    }

                    await ResourceTypeService.PostItem(currenttype);
                }


                // Delete any custom components for this resource component
                var components = await ConfigurationHelper.GetList<CustomComponent>();
                components.RemoveAll(x => x.ParentComponent == GeneralHelper.NormalizeName(item.Name, true));
                await ConfigurationHelper.WriteList<CustomComponent>(components);

                // Remove the item from the collection
                items.Remove(item);

                // Update all the sort order values to reflect the removal
                int position = 1;
                foreach (ResourceComponent thisitem in items.OrderBy(x => x.SortOrder).ToList())
                {
                    thisitem.SortOrder = position;
                    thisitem.Id = position;
                    position += 1;
                }

                // Write items to file
                await ConfigurationHelper.WriteList<ResourceComponent>(items);
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

        public static async Task<ServiceResponse> PostConfig(List<ResourceComponent> items)
        {
            try
            {
                string[] componentnames = new string[8] { "ResourceEnvironment", "ResourceInstance", "ResourceLocation", "ResourceOrg", "ResourceProjAppSvc", "ResourceType", "ResourceUnitDept", "ResourceFunction" };
                var newitems = new List<ResourceComponent>();

                // Examine the current items
                foreach (ResourceComponent item in items)
                {
                    // Check if the item is valid
                    if (!componentnames.Contains(item.Name))
                    {
                        item.IsCustom = true;
                    }
                    // Add the item to the update list
                    newitems.Add(item);
                }

                // Make sure all the component names are present
                foreach (string name in componentnames)
                {
                    if (!newitems.Exists(x => x.Name == name))
                    {
                        // Create a component object 
                        ResourceComponent newitem = new()
                        {
                            Name = name,
                            Enabled = false
                        };
                        newitems.Add(newitem);
                    }
                }

                // Determine new item ids
                int i = 1;

                foreach (ResourceComponent item in newitems.OrderByDescending(x => x.Enabled).OrderBy(x => x.SortOrder))
                {
                    item.Id = i;
                    item.SortOrder = i;
                    i += 1;
                }

                // Write items to file
                await ConfigurationHelper.WriteList<ResourceComponent>(newitems);
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
    }
}
