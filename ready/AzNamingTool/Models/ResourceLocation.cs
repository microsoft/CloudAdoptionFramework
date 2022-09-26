using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AzureNamingTool.Models
{
    public class ResourceLocation
    {
        public long Id { get; set; }
        [Required()]
        public string Name { get; set; }
        private string _ShortName;
        [Required()]
        public string ShortName
        {
            get { return _ShortName; }   // get method
            set => _ShortName = value?.ToLower();   // set method
        }
        public bool Enabled { get; set; } = true;
    }
}
