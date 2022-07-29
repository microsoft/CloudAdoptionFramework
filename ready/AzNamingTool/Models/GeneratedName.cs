namespace AzureNamingTool.Models
{
    public class GeneratedName
    {
        public long Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ResourceName { get; set; }
        public List<string[]> Components { get; set; }
    }
}
