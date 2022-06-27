using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureNamingTool.Models
{
    public class PolicyDefinition
    {
        public List<PolicyProperty> Properties { get; set; } = new List<PolicyProperty>();

        public PolicyDefinition(PolicyProperty property) 
        {
            Properties.Add(property);
        }

        public PolicyDefinition(List<PolicyProperty> property)
        {
            Properties.AddRange(property);
        }

        public override string ToString() { return "{ \"properties\": " + String.Join(",", Properties) + "}"; }
    }
}