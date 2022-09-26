using System.ComponentModel.DataAnnotations;

namespace AzureNamingTool.Models
{
    public class ResourceNameRequestWithComponents
    {
        public ResourceDelimiter ResourceDelimiter { get; set; }
        public ResourceEnvironment? ResourceEnvironment { get; set; }
        public ResourceFunction? ResourceFunction { get; set; }
        public string? ResourceInstance { get; set; }
        public ResourceLocation? ResourceLocation { get; set; }
        public ResourceOrg? ResourceOrg { get; set; }
        public ResourceProjAppSvc? ResourceProjAppSvc { get; set; }
        public ResourceType ResourceType { get; set; }
        public ResourceUnitDept? ResourceUnitDept { get; set; }
    }
}