using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureNamingTool.Models
{
    public class PolicyProperty
    {
        public PolicyProperty(String name, string description, String mode = "all")
        {
            DisplayName = name;
            Description = description;
            Mode = mode;
        }
        public string DisplayName { get; set; }
        public String PolicyType { get; set; }
        public String Mode { get; set; }
        public String Description { get; set; }
        public PropertyMetadata Metadata { get; set; } = new PropertyMetadata() { Version = "1.0.0", Category = "Azure" };
        public String PolicyRule { get; set; }

        public override string ToString() { return "{"+ PolicyRule + "}"; }
    }

    public class PropertyMetadata
    {
        public String Version { get; set; } = "1.0.0";
        public String Category { get; set; } = "Name";

        public override string ToString() { return "\"metadata\": { \"version\": \""+Version+"\", \"category\": \""+Category+"\" }"; }
     }
}