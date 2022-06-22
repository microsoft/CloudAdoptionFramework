using System.ComponentModel.DataAnnotations;

namespace AzureNamingTool.Models
{
    public class ResourceDelimiter
    {
        public long Id { get; set; }
        [Required()]
        public string Name { get; set; }
        public string Delimiter { get; set; }
        [Required()]
        public bool Enabled { get; set; }
        public int SortOrder { get; set; } = 0;
    }
}
