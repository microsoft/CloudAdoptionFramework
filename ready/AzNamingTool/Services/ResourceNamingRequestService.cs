using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System.Linq;
using System.Runtime.CompilerServices;
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
            ServiceResponse serviceResponse = new();
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
                //var resourceTypes = await ConfigurationHelper.GetList<ResourceType>();
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
                    string normalizedcomponentname = GeneralHelper.NormalizeName(component.Name, true);
                    // Check if the component is excluded for the Resource Type
                    if (!resourceType.Exclude.ToLower().Split(',').Contains(normalizedcomponentname))
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
                            if (!resourceType.Optional.ToLower().Split(',').Contains(normalizedcomponentname))
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
                        if (!ValidationHelper.CheckNumeric(lstComponents.FirstOrDefault(x => x[0] == "ResourceInstance")[1]))
                        {
                            sbMessage.Append("Resource Instance must be a numeric value.");
                            sbMessage.Append(Environment.NewLine);
                            valid = false;
                        }
                    }
                }

                // Validate the generated name for the resource type
                // CALL VALIDATION FUNCTION
                Tuple<bool, string, StringBuilder> namevalidation = ValidationHelper.ValidateGeneratedName(resourceType, name, request.ResourceDelimiter.Delimiter);

                valid = (bool)namevalidation.Item1;
                name = (string)namevalidation.Item2;
                if ((StringBuilder)namevalidation.Item3 != null)
                {
                    sbMessage.Append((StringBuilder)namevalidation.Item3);
                }


                if (valid)
                {
                    GeneratedName generatedName = new()
                    {
                        CreatedOn = DateTime.Now,
                        ResourceName = name.ToLower(),
                        Components = lstComponents
                    };
                    await GeneratedNamesService.PostItem(generatedName);
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
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                response.Message = ex.Message;
                return response;
            }
        }

        /// <summary>
        /// This function is used to generate a name by providing each componetn and the short name value. The function will validate the values to ensure they match the current configuration. 
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
                ServiceResponse serviceresponse = new();
                ResourceDelimiter resourceDelimiter = new();
                ResourceType resourceType = null;

                // Get the specified resource type
                var resourceTypes = await ConfigurationHelper.GetList<ResourceType>();
                var resourceTypesByShortName = resourceTypes.FindAll(x => x.ShortName == request.ResourceType);
                if (resourceTypesByShortName == null)
                {
                    valid = false;
                    response.Message = "ResourceType value is invalid.";
                    response.Success = false;
                    return response;
                }
                else
                {
                    if (resourceTypesByShortName.Count == 0)
                    {
                        valid = false;
                        response.Message = "ResourceType value is invalid.";
                        response.Success = false;
                        return response;
                    }
                    // Check if there are duplicates
                    if (resourceTypesByShortName.Count > 1)
                    {
                        // Check that the request includes a resource name
                        if (request.ResourceId != 0)
                        {
                            // Check if the resource value is valid
                            resourceType = resourceTypesByShortName.Find(x => x.Id == request.ResourceId);
                            if (resourceType == null)
                            {
                                valid = false;
                                response.Message = "Resource Id value is invalid.";
                                response.Success = false;
                                return response;
                            }
                        }
                        else
                        {
                            valid = false;
                            response.Message = "Your configuration contains multiple resource types for the provided short name. You must supply the Resource Id value for the resource type in your request.(Example: resourceId: 14)";
                            response.Success = false;
                            return response;
                        }
                    }
                    else
                    {
                        // Set the resource type ot the first value
                        resourceType = resourceTypesByShortName[0];
                    }
                }

                // Get the current delimiter
                serviceresponse = await ResourceDelimiterService.GetCurrentItem();
                if (serviceresponse.Success)
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

                // Make sure the passed custom component names are normalized
                if (request.CustomComponents != null)
                {
                    Dictionary<string, string> newComponents = new();
                    foreach (var cc in request.CustomComponents)
                    {
                        string value = cc.Value;
                        newComponents.Add(GeneralHelper.NormalizeName(cc.Key, true), value);
                    }
                    request.CustomComponents = newComponents;
                }

                // Get the current components
                serviceresponse = await ResourceComponentService.GetItems(false);
                var currentResourceComponents = serviceresponse.ResponseObject;

                string name = "";

                StringBuilder sbMessage = new();

                // Loop through each component
                foreach (var component in currentResourceComponents)
                {
                    string normalizedcomponentname = GeneralHelper.NormalizeName(component.Name, true);
                    if (!component.IsCustom)
                    {
                        // Check if the component is excluded for the Resource Type
                        if (!resourceType.Exclude.ToLower().Split(',').Contains(normalizedcomponentname))
                        {
                            // Attempt to retrieve value from JSON body
                            var value = GeneralHelper.GetPropertyValue(request, component.Name);

                            // Add property value to name, if exists
                            if (value != null)
                            {
                                if (value != "")
                                {
                                    // Validate that the value is a valid option for the component
                                    switch (component.Name.ToLower())
                                    {
                                        case "resourcetype":
                                            var types = await ConfigurationHelper.GetList<ResourceType>();
                                            var type = types.Find(x => x.ShortName == value);
                                            if (type == null)
                                            {
                                                valid = false;
                                                sbMessage.Append("ResourceType value is invalid. ");
                                            }
                                            break;

                                        case "resourceenvironment":
                                            var environments = await ConfigurationHelper.GetList<ResourceEnvironment>();
                                            var environment = environments.Find(x => x.ShortName == value);
                                            if (environment == null)
                                            {
                                                valid = false;
                                                sbMessage.Append("ResourceEnvironment value is invalid. ");
                                            }
                                            break;

                                        case "resourcelocation":
                                            var locations = await ConfigurationHelper.GetList<ResourceLocation>();
                                            var location = locations.Find(x => x.ShortName == value);
                                            if (location == null)
                                            {
                                                valid = false;
                                                sbMessage.Append("ResourceLocation value is invalid. ");
                                            }
                                            break;

                                        case "resourceorg":
                                            var orgs = await ConfigurationHelper.GetList<ResourceOrg>();
                                            var org = orgs.Find(x => x.ShortName == value);
                                            if (org == null)
                                            {
                                                valid = false;
                                                sbMessage.Append("Resource Type value is invalid. ");
                                            }
                                            break;

                                        case "resourceprojappsvc":
                                            var projappsvcs = await ConfigurationHelper.GetList<ResourceProjAppSvc>();
                                            var projappsvc = projappsvcs.Find(x => x.ShortName == value);
                                            if (projappsvc == null)
                                            {
                                                valid = false;
                                                sbMessage.Append("ResourceProjAppSvc value is invalid. ");
                                            }
                                            break;

                                        case "resourceunitdept":
                                            var unitdepts = await ConfigurationHelper.GetList<ResourceUnitDept>();
                                            var unitdept = unitdepts.Find(x => x.ShortName == value);
                                            if (unitdept == null)
                                            {
                                                valid = false;
                                                sbMessage.Append("ResourceUnitDept value is invalid. ");
                                            }
                                            break;

                                        case "resourcefunction":
                                            var functions = await ConfigurationHelper.GetList<ResourceFunction>();
                                            var function = functions.Find(x => x.ShortName == value);
                                            if (function == null)
                                            {
                                                valid = false;
                                                sbMessage.Append("ResourceFunction value is invalid. ");
                                            }
                                            break;
                                    }
                                    //var items = await ConfigurationHelper.GetList<ResourceComponent>();

                                    // Check if the delimeter is already ignored
                                    if ((!ignoredelimeter) && (resourceDelimiter.Delimiter != ""))
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

                                    // Add property to array for individual component validation
                                    if (!resourceType.Exclude.ToLower().Split(',').Contains(normalizedcomponentname))
                                    {
                                        lstComponents.Add(new string[] { component.Name, value });
                                    }
                                }
                                else
                                {
                                    // Check if the prop is optional
                                    if (!resourceType.Optional.ToLower().Split(',').Contains(normalizedcomponentname))
                                    {
                                        valid = false;
                                        sbMessage.Append(component.Name + " value was not provided. ");
                                    }
                                }
                            }
                            else
                            {
                                // Check if the prop is optional
                                if (!resourceType.Optional.ToLower().Split(',').Contains(normalizedcomponentname))
                                {
                                    valid = false;
                                    sbMessage.Append(component.Name + " value was not provided. ");
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!component.IsFreeText)
                        {
                            // Get the custom components data
                            serviceresponse = await CustomComponentService.GetItems();
                            var customcomponents = (List<CustomComponent>)serviceresponse.ResponseObject;
                            // Make sure the custom component has values
                            if (customcomponents.Where(x => x.ParentComponent == normalizedcomponentname).Any())
                            {
                                // Make sure the CustomComponents property was provided
                                if (!resourceType.Exclude.ToLower().Split(',').Contains(normalizedcomponentname))
                                {
                                    // Add property value to name, if exists
                                    if (request.CustomComponents != null)
                                    {
                                        // Check if the custom compoment value was provided in the request
                                        if (request.CustomComponents.ContainsKey(normalizedcomponentname))
                                        {
                                            // Get the value from the provided custom components
                                            var componentvalue = request.CustomComponents[normalizedcomponentname];
                                            if (componentvalue == null)
                                            {
                                                // Check if the prop is optional
                                                if (!resourceType.Optional.ToLower().Split(',').Contains(normalizedcomponentname))
                                                {
                                                    valid = false;
                                                    sbMessage.Append(component.Name + " value was not provided. ");
                                                }
                                            }
                                            else
                                            {
                                                // Check to make sure it is a valid custom component
                                                var customComponents = await ConfigurationHelper.GetList<CustomComponent>();
                                                var validcustomComponent = customComponents.Find(x => x.ParentComponent == normalizedcomponentname && x.ShortName == componentvalue);
                                                if (validcustomComponent == null)
                                                {
                                                    valid = false;
                                                    sbMessage.Append(component.Name + " value is not a valid custom component short name. ");
                                                }
                                                else
                                                {
                                                    if (name != "")
                                                    {
                                                        name += resourceDelimiter.Delimiter;
                                                    }

                                                    name += componentvalue;

                                                    // Add property to array for individual component validation
                                                    lstComponents.Add(new string[] { component.Name, componentvalue });
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // Check if the prop is optional
                                            if (!resourceType.Optional.ToLower().Split(',').Contains(normalizedcomponentname))
                                            {
                                                valid = false;
                                                sbMessage.Append(component.Name + " value was not provided. ");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Check if the prop is optional
                                        if (!resourceType.Optional.ToLower().Split(',').Contains(normalizedcomponentname))
                                        {
                                            valid = false;
                                            sbMessage.Append(component.Name + " value was not provided. ");
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Make sure the CustomComponents property was provided
                            if (!resourceType.Exclude.ToLower().Split(',').Contains(normalizedcomponentname))
                            {
                                // Add property value to name, if exists
                                if (request.CustomComponents != null)
                                {
                                    // Check if the custom compoment value was provided in the request
                                    if (request.CustomComponents.ContainsKey(normalizedcomponentname))
                                    {
                                        // Get the value from the provided custom components
                                        var componentvalue = request.CustomComponents[normalizedcomponentname];
                                        if (componentvalue == null)
                                        {
                                            // Check if the prop is optional
                                            if (!resourceType.Optional.ToLower().Split(',').Contains(normalizedcomponentname))
                                            {
                                                valid = false;
                                                sbMessage.Append(component.Name + " value was not provided. ");
                                            }
                                        }
                                        else
                                        {
                                            if (name != "")
                                            {
                                                name += resourceDelimiter.Delimiter;
                                            }

                                            name += componentvalue;

                                            // Add property to array for individual component validation
                                            lstComponents.Add(new string[] { component.Name, componentvalue });
                                        }
                                    }
                                    else
                                    {
                                        // Check if the prop is optional
                                        if (!resourceType.Optional.ToLower().Split(',').Contains(normalizedcomponentname))
                                        {
                                            valid = false;
                                            sbMessage.Append(component.Name + " value was not provided. ");
                                        }
                                    }
                                }
                                else
                                {
                                    // Check if the prop is optional
                                    if (!resourceType.Optional.ToLower().Split(',').Contains(normalizedcomponentname))
                                    {
                                        valid = false;
                                        sbMessage.Append(component.Name + " value was not provided. ");
                                    }
                                }
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
                        if (!ValidationHelper.CheckNumeric(lstComponents.FirstOrDefault(x => x[0] == "ResourceInstance")[1]))
                        {
                            sbMessage.Append("Resource Instance must be a numeric value.");
                            sbMessage.Append(Environment.NewLine);
                            valid = false;
                        }
                    }
                }

                // Validate the generated name for the resource type
                // CALL VALIDATION FUNCTION
                Tuple<bool, string, StringBuilder> namevalidation = ValidationHelper.ValidateGeneratedName(resourceType, name, resourceDelimiter.Delimiter);

                valid = (bool)namevalidation.Item1;
                name = (string)namevalidation.Item2;
                if ((StringBuilder)namevalidation.Item3 != null)
                {
                    sbMessage.Append((StringBuilder)namevalidation.Item3);
                }

                if (valid)
                {
                    bool nameallowed = true;
                    // Check if duplicate names are allowed
                    if (!ConfigurationHelper.VerifyDuplicateNamesAllowed())
                    {
                        // Check if the name already exists
                        serviceresponse = await GeneratedNamesService.GetItems();
                        var names = (List<GeneratedName>)serviceresponse.ResponseObject;
                        if (names.Where(x => x.ResourceName == name).Any())
                        {
                            nameallowed = false;
                        }
                    }
                    if (nameallowed)
                    {
                        GeneratedName generatedName = new()
                        {
                            CreatedOn = DateTime.Now,
                            ResourceName = name.ToLower(),
                            Components = lstComponents,
                            ResourceTypeName = resourceType.Resource,
                            User = request.CreatedBy
                        };
                        ServiceResponse responseGenerateName = await GeneratedNamesService.PostItem(generatedName);
                        if (responseGenerateName.Success)
                        {
                            response.Success = true;
                            response.ResourceName = name.ToLower();
                            response.Message = sbMessage.ToString();
                            response.resourceNameDetails = generatedName;

                            // Check if the GenerationWebhook is configured
                            String webhook = ConfigurationHelper.GetAppSetting("GenerationWebhook", true);
                            if (!String.IsNullOrEmpty(webhook))
                            {
                                // Asynchronously post to the webhook
                                ConfigurationHelper.PostToGenerationWebhook(webhook, generatedName);
                            }
                        }
                        else
                        {
                            response.Success = false;
                            response.ResourceName = "***RESOURCE NAME NOT GENERATED***";
                            response.Message = "There was an error generating the name. Please try again.";
                        }
                    }
                    else
                    {
                        response.Success = false;
                        response.ResourceName = "***RESOURCE NAME NOT GENERATED***";
                        response.Message = "The name (" + name + ") you are trying to generate already exists. Please select different component options and try again.";
                    }
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
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                response.Message = ex.Message;
                return response;
            }
        }
    }
}
