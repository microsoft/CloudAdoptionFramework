using AzureNamingTool.Helpers;
using System.ComponentModel;

namespace AzureNamingTool.Models
{
    public class ResponseMessage
    {
        public MessageTypesEnum Type { get; set; } = MessageTypesEnum.INFORMATION;
        public string Header { get; set; } = "Message";
        public string Message { get; set; }
        public string? MessageDetails { get; set; }
    }
}
