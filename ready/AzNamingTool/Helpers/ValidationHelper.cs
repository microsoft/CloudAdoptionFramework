using AzureNamingTool.Models;
using AzureNamingTool.Services;
using System.Text.RegularExpressions;
using System.Text;

namespace AzureNamingTool.Helpers
{
    public class ValidationHelper
    {
        public static bool ValidatePassword(string text)
        {
            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasMinimum8Chars = new Regex(@".{8,}");

            var isValidated = hasNumber.IsMatch(text) && hasUpperChar.IsMatch(text) && hasMinimum8Chars.IsMatch(text);

            return isValidated;
        }

        public static async Task<bool> ValidateShortName(string type, string value, string parentcomponent = null)
        {
            bool valid = false;
            try
            {
                ResourceComponent resourceComponent = new();
                List<ResourceComponent> resourceComponents = new();
                ServiceResponse serviceResponse;

                // Get the current components
                serviceResponse = await ResourceComponentService.GetItems(true);
                if (serviceResponse.Success)
                {
                    resourceComponents = (List<ResourceComponent>)serviceResponse.ResponseObject;

                    // Check if it's a custom component
                    if (type == "CustomComponent")
                    {
                        resourceComponent = resourceComponents.Find(x => GeneralHelper.NormalizeName(x.Name, true) == GeneralHelper.NormalizeName(parentcomponent, true));
                    }
                    else
                    {
                        resourceComponent = resourceComponents.Find(x => x.Name == type);
                    }

                    if (resourceComponent != null)
                    {
                        // Check if the name mathces the length requirements for the component
                        if ((value.Length >= (Convert.ToInt32(resourceComponent.MinLength)) && (value.Length <= Convert.ToInt32(resourceComponent.MaxLength))))
                        {
                            valid = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
            return valid;
        }

        public static Tuple<bool, string, StringBuilder> ValidateGeneratedName(Models.ResourceType resourceType, string name, string delimiter)
        {
            try
            {
                bool valid = true;
                StringBuilder sbMessage = new();
                // Check regex
                // Validate the name against the resource type regex
                Regex regx = new(resourceType.Regx);
                Match match = regx.Match(name);
                if (!match.Success)
                {
                    // Strip the delimiter in case that is causing the issue
                    if (delimiter != "")
                    {
                        // Strip the delimiter in case that is causing the issue
                        name = name.Replace(delimiter, "");

                        Match match2 = regx.Match(name);
                        if (!match2.Success)
                        {
                            sbMessage.Append("Regex failed - Please review the Resource Type Naming Guidelines.");
                            sbMessage.Append(Environment.NewLine);
                            valid = false;
                        }
                        else
                        {
                            sbMessage.Append("The specified delimiter is not allowed for this resource type and has been removed.");
                            sbMessage.Append(Environment.NewLine);
                        }
                    }
                    else
                    {
                        sbMessage.Append("The specified delimiter is not allowed for this resource type and has been removed.");
                        sbMessage.Append(Environment.NewLine);
                    }
                }

                // Check min length
                if (int.TryParse(resourceType.LengthMin, out _))
                {
                    if (name.Length < int.Parse(resourceType.LengthMin))
                    {
                        sbMessage.Append("Generated name is less than the minimum length for the selected resource type.");
                        sbMessage.Append(Environment.NewLine);
                        valid = false;
                    }
                }

                // Check max length
                if (int.TryParse(resourceType.LengthMax, out _))
                {
                    if (name.Length > int.Parse(resourceType.LengthMax))
                    {
                        // Strip the delimiter in case that is causing the issue
                        name = name.Replace(delimiter, "");
                        if (name.Length > int.Parse(resourceType.LengthMax))
                        {
                            sbMessage.Append("Generated name is more than the maximum length for the selected resource type.");
                            sbMessage.Append(Environment.NewLine);
                            sbMessage.Append("Please remove any optional components or contact your admin to update the required components for this resource type.");
                            sbMessage.Append(Environment.NewLine);
                            valid = false;
                        }
                        else
                        {
                            sbMessage.Append("Generated name with the selected delimiter is more than the maximum length for the selected resource type. The delimiter has been removed.");
                            sbMessage.Append(Environment.NewLine);
                        }
                    }
                }

                // Check invalid characters
                if (resourceType.InvalidCharacters != "")
                {
                    // Loop through each character
                    foreach (char c in resourceType.InvalidCharacters)
                    {
                        // Check if the name contains the character
                        if (name.Contains(c))
                        {
                            sbMessage.Append("Name cannot contain the following character: " + c);
                            sbMessage.Append(Environment.NewLine);
                            valid = false;
                        }
                    }
                }

                // Check start character
                if (resourceType.InvalidCharactersStart != "")
                {
                    // Loop through each character
                    foreach (char c in resourceType.InvalidCharactersStart)
                    {
                        // Check if the name contains the character
                        if (name.StartsWith(c))
                        {
                            sbMessage.Append("Name cannot start with the following character: " + c);
                            sbMessage.Append(Environment.NewLine);
                            valid = false;
                        }
                    }
                }

                // Check start character
                if (resourceType.InvalidCharactersEnd != "")
                {
                    // Loop through each character
                    foreach (char c in resourceType.InvalidCharactersEnd)
                    {
                        // Check if the name contains the character
                        if (name.EndsWith(c))
                        {
                            sbMessage.Append("Name cannot end with the following character: " + c);
                            sbMessage.Append(Environment.NewLine);
                            valid = false;
                        }
                    }
                }

                // Check consecutive character
                if (resourceType.InvalidCharactersConsecutive != "")
                {
                    // Loop through each character
                    foreach (char c in resourceType.InvalidCharactersConsecutive)
                    {
                        // Check if the name contains the character
                        char current = name[0];
                        for (int i = 1; i < name.Length; i++)
                        {
                            char next = name[i];
                            if ((current == next) && (current == c))
                            {
                                sbMessage.Append("Name cannot contain the following consecutive character: " + next);
                                sbMessage.Append(Environment.NewLine);
                                valid = false;
                                break;
                            }
                            current = next;
                        }
                    }
                }
                return new Tuple<bool, string, StringBuilder>(valid, name, sbMessage);
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return new Tuple<bool, string, StringBuilder>(false, name, new StringBuilder("There was a problem validating the name."));
            }
        }

        public static bool CheckNumeric(string value)
        {
            Regex regx = new("^[0-9]+$");
            Match match = regx.Match(value);
            return match.Success;
        }

        public static bool CheckAlphanumeric(string value)
        {
            Regex regx = new("^[a-zA-Z0-9]+$");
            Match match = regx.Match(value);
            return match.Success;
        }
    }
}
