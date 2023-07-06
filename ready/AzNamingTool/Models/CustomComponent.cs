using System.ComponentModel.DataAnnotations;

namespace AzureNamingTool.Models
{
    public class CustomComponent
    {
        public long Id { get; set; }
        [Required()]
        public string ParentComponent { get; set; }
        [Required()]
        public string Name { get; set; }
        private string _ShortName;
        [Required()]
        public string ShortName
        {
            get { return _ShortName; }   // get method
            set => _ShortName = value?.ToLower();   // set method
        }
        public int SortOrder { get; set; } = 0;
        public string MinLength { get; set; } = "1";
        public string MaxLength { get; set; } = "10";
    }
}
