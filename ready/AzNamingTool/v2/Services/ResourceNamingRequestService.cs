using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using Microsoft.AspNetCore.Components;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace AzureNamingTool.Services
{
    public class ResourceNamingRequestService
    {
        public static async Task<ResourceNameResponse> RequestName(ResourceNameRequest request)
        {
            ResourceNameResponse response = new();
            response.Success = false;

            try
            {
                bool valid = true;
                bool ignoredelimeter = false;
                List<Tuple<string, string>> lstComponents = new();

                // Get the specified resource type
                //var resourceTypes = await GeneralHelper.GetList<ResourceType>();
                //var resourceType = resourceTypes.Find(x => x.Id == request.ResourceType);
                var resourceType = request.ResourceType;

                // Check static value
                if (resourceType.StaticValues != "")
                {
                    // Return the static value and message and stop generation.
                    response.ResourceName = resourceType.StaticValues;
                    response.Message = "The requested Resource Type name is considered a static value with specific requirements. Please refer to https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/resource-name-rules for additional information.";
                    response.Success = true;
                    return response;
                }

                // Get the components
                ServiceResponse serviceresponse = new();
                serviceresponse = await ResourceComponentService.GetItems(false);
                var currentResourceComponents = serviceresponse.ResponseObject;
                dynamic d = request;

                string name = "";

                StringBuilder sbMessage = new();

                // Loop through each component
                foreach (var component in currentResourceComponents)
                {
                    // Check if the component is excluded for the Resource Type
                    if (!resourceType.Exclude.ToLower().Contains(component.Name.ToLower().Replace("resource", ""), StringComparison.CurrentCulture))
                    {
                        // Attempt to retrieve value from JSON body
                        var prop = GeneralHelper.GetPropertyValue(d, component.Name);
                        string value = null;

                        // Add property value to name, if exists
                        if (prop != null)
                        {
                            if (component.Name == "ResourceInstance")
                            {
                                value = prop;
                            }
                            else
                            {
                                value = prop.GetType().GetProperty("ShortName").GetValue(prop, null).ToLower();
                            }

                            // Check if the delimeter is already ignored
                            if (!ignoredelimeter)
                            {
                                // Check if delimeter is an invalid character
                                if (resourceType.InvalidCharacters != "")
                                {
                                    if (!resourceType.InvalidCharacters.Contains(request.ResourceDelimiter.Delimiter))
                                    {
                                        if (name != "")
                                        {
                                            name += request.ResourceDelimiter.Delimiter;
                                        }
                                    }
                                    else
                                    {
                                        // Add message about delimeter not applied
                                        sbMessage.Append("The specified delimiter is not allowed for this resource type and has been removed.");
                                        sbMessage.Append(Environment.NewLine);
                                        ignoredelimeter = true;
                                    }
                                }
                                else
                                {
                                    // Deliemeter is valid so add it
                                    if (name != "")
                                    {
                                        name += request.ResourceDelimiter.Delimiter;
                                    }
                                }
                            }

                            name += value;

                            // Add property to aray for indivudal component validation
                            if (component.Name == "ResourceType")
                            {
                                lstComponents.Add(new Tuple<string, string>(component.Name, prop.Resource + " (" + value + ")"));
                            }
                            else
                            {
                                if (component.Name == "ResourceInstance")
                                {
                                    lstComponents.Add(new Tuple<string, string>(component.Name, prop));
                                }
                                else
                                {
                                    lstComponents.Add(new Tuple<string, string>(component.Name, prop.Name + " (" + value + ")"));
                                }
                            }
                        }
                        else
                        {
                            // Check if the prop is optional
                            if (!resourceType.Optional.ToLower().Contains(component.Name.ToLower().Replace("resource", "")))
                            {
                                valid = false;
                                break;
                            }
                        }
                    }
                }

                // Check if the required component were supplied
                if (!valid)
                {
                    response.ResourceName = "***RESOURCE NAME NOT GENERATED***";
                    response.Message = "You must supply the required components.";
                    return response;
                }

                // Check the Resource Instance value to ensure it's only nmumeric
                if (lstComponents.FirstOrDefault(x => x.Item1 == "ResourceInstance") != null)
                {
                    if (lstComponents.FirstOrDefault(x => x.Item1 == "ResourceInstance").Item2 != null)
                    {
                        if (!GeneralHelper.CheckNumeric(lstComponents.FirstOrDefault(x => x.Item1 == "ResourceInstance").Item2))
                        {
                            sbMessage.Append("Resource Instance must be a numeric value.");
                            sbMessage.Append(Environment.NewLine);
                            valid = false;
                        }
                    }
                }

                // Validate the generated name for the resource type
                // CALL VALIDATION FUNCTION
                Tuple<bool,string, StringBuilder> namevalidation = GeneralHelper.ValidateGeneratedName(resourceType, name, request.ResourceDelimiter.Delimiter);

                valid = (bool)namevalidation.Item1;
                name = (string)namevalidation.Item2;
                if((StringBuilder)namevalidation.Item3 != null)
                {
                    sbMessage.Append((StringBuilder)namevalidation.Item3);
                }


                if (valid)
                {
                    GeneratedName generatedName = new GeneratedName()
                    {
                        CreatedOn = DateTime.Now,
                        ResourceName = name.ToLower(),
                        Components = lstComponents
                    };
                    LogGeneratedName(generatedName);
                    response.Success = true;
                    response.ResourceName = name.ToLower();
                    response.Message = sbMessage.ToString();
                    return response;
                }
                else
                {
                    response.ResourceName = "***RESOURCE NAME NOT GENERATED***";
                    response.Message = sbMessage.ToString();
                    return response;
                }
            }
            catch (Exception ex)
            {
                GeneralHelper.LogAdminMessage("ERROR", ex.Message);
                response.Message = ex.Message;
                return response;
            }
        }

        public static async Task<List<GeneratedName>> GetGeneratedNames()
        {
            List<GeneratedName> lstGeneratedNames = new();
            try
            {
                string data = await FileSystemHelper.ReadFile("generatednames.json");
                var items = new List<GeneratedName>();
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };
                lstGeneratedNames = JsonSerializer.Deserialize<List<GeneratedName>>(data, options).OrderByDescending(x => x.CreatedOn).ToList();
            }
            catch (Exception ex)
            {
                GeneralHelper.LogAdminMessage("ERROR", ex.Message);
            }
            return lstGeneratedNames;
        }

        public static async void LogGeneratedName(GeneratedName lstGeneratedName)
        {
            try
            {
                // Log the created name
                var lstGeneratedNames = new List<GeneratedName>();
                lstGeneratedNames = await GetGeneratedNames();

                if (lstGeneratedNames.Count > 0)
                {
                    lstGeneratedName.Id = lstGeneratedNames.Max(x => x.Id) + 1;
                }
                else
                {
                    lstGeneratedName.Id = 1;
                }

                lstGeneratedNames.Add(lstGeneratedName);
                var jsonGeneratedNames = JsonSerializer.Serialize(lstGeneratedNames);
                await FileSystemHelper.WriteFile("generatednames.json", jsonGeneratedNames);
            }
            catch (Exception ex)
            {
                GeneralHelper.LogAdminMessage("ERROR", ex.Message);
            }
        }
        public static async Task PurgeGeneratedNames()
        {
            try
            {
                await FileSystemHelper.WriteFile("generatednames.json", "[]");
            }
            catch (Exception ex)
            {
                GeneralHelper.LogAdminMessage("ERROR", ex.Message);
            }
        }
    }
}
