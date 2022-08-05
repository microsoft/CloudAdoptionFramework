using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace AzureNamingTool.Services
{
    public class ResourceNamingRequestService
    {
        /// <summary>
        /// This function will generate a resoure type name for specifed component values. This function requires full definition for all components. It is recommended to use the ResourceNameRequest API function for name generation.   
        /// </summary>
        /// <param name="request"></param>
        /// <returns>ResourceNameResponse - Response of name generation</returns>
        public static async Task<ResourceNameResponse> RequestNameWithComponents(ResourceNameRequestWithComponents request)
        {
            ResourceNameResponse response = new()
            {
                Success = false
            };

            try
            {
                bool valid = true;
                bool ignoredelimeter = false;
                List<string[]> lstComponents = new();

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
                                lstComponents.Add(new string[] { component.Name, prop.Resource + " (" + value + ")" });
                            }
                            else
                            {
                                if (component.Name == "ResourceInstance")
                                {
                                    lstComponents.Add(new string[] { component.Name, prop });
                                }
                                else
                                {
                                    lstComponents.Add(new string[] { component.Name, prop.Name + " (" + value + ")" });
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
                if (lstComponents.FirstOrDefault(x => x[0] == "ResourceInstance") != null)
                {
                    if (lstComponents.FirstOrDefault(x => x[0] == "ResourceInstance")[1] != null)
                    {
                        if (!GeneralHelper.CheckNumeric(lstComponents.FirstOrDefault(x => x[0] == "ResourceInstance")[1]))
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
                    LogHelper.LogGeneratedName(generatedName);
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
                LogHelper.LogAdminMessage("ERROR", ex.Message);
                response.Message = ex.Message;
                return response;
            }
        }

        /// <summary>
        /// This function is used to generate a name by providing each componetn and teh short name value. The function will validate the values to ensure they match the current configuration. 
        /// </summary>
        /// <param name="request"></param>
        /// <returns>ResourceNameResponse - Response of name generation</returns>
        public static async Task<ResourceNameResponse> RequestName(ResourceNameRequest request)
        {
            ResourceNameResponse response = new()
            {
                Success = false
            };

            try
            {
                bool valid = true;
                bool ignoredelimeter = false;
                List<string[]> lstComponents = new();
                string delimiterstring = "";
                ServiceResponse serviceresponse = new();
                ResourceDelimiter resourceDelimiter = new();

                // Get the specified resource type
                var resourceTypes = await GeneralHelper.GetList<ResourceType>();
                var resourceType = resourceTypes.Find(x => x.ShortName == request.ResourceType);
                if(resourceType == null)
                {
                    valid = false;
                    response.Message = "ResourceType value is invalid.";
                    response.Success = false;
                    return response;
                }

                // Get the current delimeter
                serviceresponse = await ResourceDelimiterService.GetItem();
                if(serviceresponse.Success)
                {
                    resourceDelimiter = (ResourceDelimiter)serviceresponse.ResponseObject;
                }
                else
                {
                    valid = false;
                    response.Message = "Delimiter value could not be set.";
                    response.Success = false;
                    return response;
                }
                
                // Check static value
                if (resourceType.StaticValues != "")
                {
                    // Return the static value and message and stop generation.
                    response.ResourceName = resourceType.StaticValues;
                    response.Message = "The requested Resource Type name is considered a static value with specific requirements. Please refer to https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/resource-name-rules for additional information.";
                    response.Success = true;
                    return response;
                }

                // Get the current components
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
                        var value = GeneralHelper.GetPropertyValue(d, component.Name);

                        // Add property value to name, if exists
                        if (value != null)
                        {
                            // Validate that the value is a valid  option for the component
                            switch (component.Name.ToLower())
                            {
                                case "resourcetype":
                                    var types = await GeneralHelper.GetList<ResourceType>();
                                    var type = types.Find(x => x.ShortName == value);
                                    if (type == null)
                                    {
                                        valid = false;
                                        sbMessage.Append("ResourceType value is invalid. ");
                                    }
                                    break;

                                case "resourceenvironment":
                                    var environments = await GeneralHelper.GetList<ResourceEnvironment>();
                                    var environment = environments.Find(x => x.ShortName == value);
                                    if (environment == null)
                                    {
                                        valid = false;
                                        sbMessage.Append("ResourceEnvironment value is invalid. ");
                                    }
                                    break;

                                case "resourcelocation":
                                    var locations = await GeneralHelper.GetList<ResourceLocation>();
                                    var location = locations.Find(x => x.ShortName == value);
                                    if (location == null)
                                    {
                                        valid = false;
                                        sbMessage.Append("ResourceLocation value is invalid. ");
                                    }
                                    break;

                                case "resourceorg":
                                    var orgs = await GeneralHelper.GetList<ResourceOrg>();
                                    var org = orgs.Find(x => x.ShortName == value);
                                    if (org == null)
                                    {
                                        valid = false;
                                        sbMessage.Append("Resource Type value is invalid. ");
                                    }
                                    break;

                                case "resourceprojappsvc":
                                    var projappsvcs = await GeneralHelper.GetList<ResourceProjAppSvc>();
                                    var projappsvc = projappsvcs.Find(x => x.ShortName == value);
                                    if (projappsvc == null)
                                    {
                                        valid = false;
                                        sbMessage.Append("ResourceProjAppSvc value is invalid. ");
                                    }
                                    break;

                                case "resourceunitdept":
                                    var unitdepts = await GeneralHelper.GetList<ResourceUnitDept>();
                                    var unitdept = unitdepts.Find(x => x.ShortName == value);
                                    if (unitdept == null)
                                    {
                                        valid = false;
                                        sbMessage.Append("ResourceUnitDept value is invalid. ");
                                    }
                                    break;

                                case "resourcefunction":
                                    var functions = await GeneralHelper.GetList<ResourceFunction>();
                                    var function = functions.Find(x => x.ShortName == value);
                                    if (function == null)
                                    {
                                        valid = false;
                                        sbMessage.Append("ResourceFunction value is invalid. ");
                                    }
                                    break;

                            }
                            var items = await GeneralHelper.GetList<ResourceComponent>();

                            // Check if the delimeter is already ignored
                            if (!ignoredelimeter)
                            {
                                // Check if delimeter is an invalid character
                                if (resourceType.InvalidCharacters != "")
                                {
                                    if (!resourceType.InvalidCharacters.Contains(resourceDelimiter.Delimiter))
                                    {
                                        if (name != "")
                                        {
                                            name += resourceDelimiter.Delimiter;
                                        }
                                    }
                                    else
                                    {
                                        // Add message about delimeter not applied
                                        sbMessage.Append("The specified delimiter is not allowed for this resource type and has been removed. ");
                                        ignoredelimeter = true;
                                    }
                                }
                                else
                                {
                                    // Deliemeter is valid so add it
                                    if (name != "")
                                    {
                                        name += resourceDelimiter.Delimiter;
                                    }
                                }
                            }

                            name += value;

                            // Add property to aray for individual component validation
                            lstComponents.Add(new string[] { component.Name, value });
                        }
                        else
                        {
                            // Check if the prop is optional
                            if (!resourceType.Optional.ToLower().Contains(component.Name.ToLower().Replace("resource", "")))
                            {
                                valid = false;
                                sbMessage.Append(component.Name + " value was not provided. ");
                            }
                        }
                    }
                }

                // Check if the required component were supplied
                if (!valid)
                {
                    response.ResourceName = "***RESOURCE NAME NOT GENERATED***";
                    response.Message = "You must supply the required components. " + sbMessage.ToString();
                    return response;
                }

                // Check the Resource Instance value to ensure it's only nmumeric
                if (lstComponents.FirstOrDefault(x => x[0] == "ResourceInstance") != null)
                {
                    if (lstComponents.FirstOrDefault(x => x[0] == "ResourceInstance")[1] != null)
                    {
                        if (!GeneralHelper.CheckNumeric(lstComponents.FirstOrDefault(x => x[0] == "ResourceInstance")[1]))
                        {
                            sbMessage.Append("Resource Instance must be a numeric value.");
                            sbMessage.Append(Environment.NewLine);
                            valid = false;
                        }
                    }
                }

                // Validate the generated name for the resource type
                // CALL VALIDATION FUNCTION
                Tuple<bool, string, StringBuilder> namevalidation = GeneralHelper.ValidateGeneratedName(resourceType, name, resourceDelimiter.Delimiter);

                valid = (bool)namevalidation.Item1;
                name = (string)namevalidation.Item2;
                if ((StringBuilder)namevalidation.Item3 != null)
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
                    LogHelper.LogGeneratedName(generatedName);
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
                LogHelper.LogAdminMessage("ERROR", ex.Message);
                response.Message = ex.Message;
                return response;
            }
        }
    }
}
