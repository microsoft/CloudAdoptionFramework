using AzureNamingTool.Models;
using AzureNamingTool.Pages;
using System.Collections.Generic;

namespace AzureNamingTool.Models
{
    public class ConfigurationData
    {
        public List<ResourceComponent> ResourceComponents { get; set; }
        public List<ResourceDelimiter> ResourceDelimiters { get; set; }
        public List<ResourceEnvironment> ResourceEnvironments { get; set; }
        public List<ResourceLocation> ResourceLocations { get; set; }
        public List<ResourceOrg> ResourceOrgs { get; set; }
        public List<ResourceProjAppSvc> ResourceProjAppSvcs { get; set; }
        public List<ResourceType> ResourceTypes { get; set; }
        public List<ResourceUnitDept> ResourceUnitDepts { get; set; }
        public List<ResourceFunction> ResourceFunctions { get; set; }
        public List<CustomComponent> CustomComponents { get; set; }
        public List<GeneratedName> GeneratedNames { get; set; }
        public List<AdminLogMessage> AdminLogs { get; set; }
        public List<AdminUser> AdminUsers { get; set; }

        public string? SALTKey { get; set; }
        public string? AdminPassword { get; set; }
        public string? APIKey { get; set; }
        public string? DismissedAlerts { get; set; }
        public string? DuplicateNamesAllowed { get; set; } = "false";
        public string? GenerationWebhook { get; set; } = string.Empty;
        public string? ConnectivityCheckEnabled { get; set; } = "true";
        public string? IdentityHeaderName { get; set; } = "X-MS-CLIENT-PRINCIPAL-NAME";
        public string? ResourceTypeEditingAllowed { get; set; } = "false";
    }
}
