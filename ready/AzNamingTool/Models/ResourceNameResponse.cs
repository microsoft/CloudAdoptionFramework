namespace AzureNamingTool.Models
{
    public class ResourceNameResponse
    {
        public string ResourceName { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public GeneratedName? resourceNameDetails { get; set; }
    }
}
