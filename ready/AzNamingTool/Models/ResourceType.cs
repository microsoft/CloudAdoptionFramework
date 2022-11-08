using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AzureNamingTool.Models
{
    public class ResourceType
    {
        public long Id { get; set; }
        [Required()]
        public string Resource {  get; set; }
        public string Optional { get; set; }
        public string Exclude { get; set; }
        public string Property { get; set; }
        private string _ShortName;
        [JsonPropertyName("ShortName")]
        public string ShortName
        {
            get { return _ShortName; }   // get method
            set => _ShortName = value?.ToLower();   // set method
        }
        public string Scope { get; set; }
        public string LengthMin { get; set; }
        public string LengthMax { get; set; }
        public string ValidText { get; set; }
        public string InvalidText { get; set; }
        public string InvalidCharacters { get; set; }
        public string InvalidCharactersStart { get; set; }
        public string InvalidCharactersEnd { get; set; }
        public string InvalidCharactersConsecutive{ get; set; }
        public string Regx { get; set; }
        public string StaticValues { get; set; }
        public bool Enabled { get; set; } = true;
    }
}
