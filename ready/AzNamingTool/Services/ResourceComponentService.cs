using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Services
{
    public class ResourceComponentService
    {
        private static ServiceResponse serviceresponse = new();

        public static async Task<ServiceResponse> GetItems(bool admin)
        {
            try
            {
                var items = await GeneralHelper.GetList<ResourceComponent>();
                if (!admin)
                {
                    serviceresponse.ResponseObject = items.Where(x => x.Enabled == true).OrderBy(y => y.SortOrder).ToList();
                }
                else
                {
                    serviceresponse.ResponseObject = items.OrderBy(y => y.SortOrder).OrderByDescending(y => y.Enabled).ToList();
                }
                serviceresponse.Success = true;
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
                serviceresponse.Success = false;
                serviceresponse.ResponseObject = ex;
            }
            return serviceresponse;
        }

        public static async Task<ServiceResponse> PostItem(ResourceComponent item)
        {
            try
            {
                // Get list of items
                var items = await GeneralHelper.GetList<ResourceComponent>();

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
                    if (items.Exists(x => x.Id == item.Id))
                    {
                        // Remove the updated item from the list
                        var existingitem = items.Find(x => x.Id == item.Id);
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
                foreach (ResourceComponent thisitem in items.OrderBy(x => x.SortOrder).OrderByDescending(x => x.Enabled).ToList())
                {
                    thisitem.SortOrder = position;
                    position += 1;
                }

                // Write items to file
                await GeneralHelper.WriteList<ResourceComponent>(items);
                serviceresponse.Success = true;
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
                serviceresponse.Success = false;
                serviceresponse.ResponseObject = ex;
            }
            return serviceresponse;
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
                    if (componentnames.Contains(item.Name))
                    {
                        // Add the item to the update list
                        newitems.Add(item);
                    }
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
                await GeneralHelper.WriteList<ResourceComponent>(newitems);
                serviceresponse.Success = true;
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
                serviceresponse.Success = false;
                serviceresponse.ResponseObject = ex;
            }
            return serviceresponse;
        }
    }
}
