using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureNamingTool.Models
{
    public class PolicyRuleFactory
    {
        internal static string GetNameValidationRules(List<PolicyRule> policies, Char delimeter, PolicyEffects effect = PolicyEffects.Deny)
        {
            var ifHeader = "\"if\": {\"allOf\": [";
            var policyGroups = policies.GroupBy(x => String.Join(',', x.Group));
            var ifContent = GenerateConditions(policyGroups);
            var ifFooter = "]}";
            var thenContent = ", \"then\": {\"effect\":\"" + effect.ToString().ToLower() + "\"}";

            return "\"policyRule\": {" + ifHeader + ifContent + ifFooter + thenContent + "}";
        }

        static string GetMainCondition(List<PolicyRule> conditions)
        {
            return "{\"not\": { \"value\": \"[substring(field('name'), " + conditions.First().StartIndex + ", " + conditions.First().Length + ")]\",\"in\": [" + String.Join(',', conditions.Select(x => "\"" + x.Name + "\"").Distinct()) + "]}}";
        }

        private static string GenerateConditions(IEnumerable<IGrouping<string, PolicyRule>> policyGroups, int level = 1, int startIndex = 0)
        {
            String result = String.Empty;
            var list = policyGroups.Where(x => x.Key.StartsWith($"{level},{startIndex}")).ToList();
            foreach (var levelConditions in list)
            {
                var header = "{\"allOf\": [";
                var mainCondition = GetMainCondition(levelConditions.ToList());
                var insideConditions = String.Empty;
                var fullLength = levelConditions.FirstOrDefault().FullLength;
                var startIndexes = policyGroups.Where(x => x.Key.StartsWith($"{level + 1}")).Select(x => Convert.ToInt32(x.Key.Split(',')[1])).Where(x => x == fullLength).Distinct().ToList();
                foreach (var nextStartIndex in startIndexes)
                {
                    insideConditions += GenerateConditions(policyGroups, level + 1, nextStartIndex);
                    if (startIndexes.Last() != nextStartIndex)
                        insideConditions += ",";
                }

                var footer = "]}";

                if (list.Last().Key != levelConditions.Key)
                    footer += ",";

                result += header + mainCondition + (insideConditions == String.Empty ? String.Empty : "," + insideConditions) + footer;
            }

            return result;
        }
    }
}
