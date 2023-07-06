namespace AzureNamingTool.Models
{
    public class AdminLogMessage
    {
        public long? Id { get; set; } = 0;
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string Title { get; set; }
        public string Message { get; set; }
        public string? Source { get; set; } = "System";
    }
}