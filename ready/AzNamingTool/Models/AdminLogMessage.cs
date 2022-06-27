namespace AzureNamingTool.Models
{
    public class AdminLogMessage
    {
        public long Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
    }
}
