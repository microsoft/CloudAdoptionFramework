using System.ComponentModel.DataAnnotations;

namespace AzureNamingTool.Models
{
    public class ResourceComponent
    {
        public long Id { get; set; }
        [Required()]
        public string Name { get; set; }
        [Required()]
        public string DisplayName { get; set; }
        [Required()]
        public bool Enabled { get; set; }
        public int SortOrder { get; set; } = 0;
    }
}
