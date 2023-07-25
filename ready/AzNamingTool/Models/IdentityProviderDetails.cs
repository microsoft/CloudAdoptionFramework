namespace AzureNamingTool.Models
{
    public class IdentityProviderDetails
    {
        public string CurrentUser { get; set; } = "System";
        public string? CurrentIdentityProvider { get; set; }
    }
}
