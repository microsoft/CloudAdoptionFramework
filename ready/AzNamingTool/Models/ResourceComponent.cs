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
        public bool IsCustom { get; set; } = false;
        public bool IsFreeText { get; set; } = false;
        public string? MinLength { get; set; }
        public string? MaxLength { get; set; }
    }
}
