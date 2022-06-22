using AzureNamingTool.Models;
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
        public string SALTKey { get; set; }
        public string AdminPassword { get; set; }
        public string APIKey { get; set; }
    }
}
