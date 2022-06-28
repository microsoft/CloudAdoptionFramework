using System;
using System.Linq;

namespace AzureNamingTool.Models
{
    public class PolicyRule
    {
        public PolicyRule(String fullName, Char delimeter = '-')
        {
            FullName = fullName;
            Delimeter = delimeter;
        }
        public Char Delimeter { get; set; }
        public String FullName { get; set; }
        public int Level { get { return FullName.Count(p => p == Delimeter); } }
        public String Name { get { return FullName.Substring(StartIndex, LastIndex - StartIndex); } }
        public int Length { get { return Name.Length; } }
        public int FullLength { get { return FullName.Length-1; } }
        public int StartIndex { get { return BeforeLastIndexOf(FullName, Delimeter); } }
        public int LastIndex { get { return FullName.LastIndexOf(Delimeter); } }
        int BeforeLastIndexOf(String value, Char toFind)
        {
            int result = 0;
            for (int i = value.LastIndexOf(toFind) - 1; i > 0; i--)
            {
                if (value[i] == toFind)
                {
                    result = i+1;
                    break;
                }
            }

            return result;
        }
        public int[] Group { get { return new int[] { Level, StartIndex, Length }; } }
        public override int GetHashCode()
        {
            return (String.Join(',', Group) + Name).GetHashCode();
        }
    }
  }