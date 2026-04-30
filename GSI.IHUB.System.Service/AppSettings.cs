namespace GSI.IHUB.System.Service;

public class AppSettings
{
    public string KeyVaultUri { get; set; } = string.Empty;
    public string ExternalApiBaseUrl { get; set; } = string.Empty;
    public string ExternalApiSubscriptionKey { get; set; } = string.Empty;
    public string ExternalApiVersion { get; set; } = string.Empty;
    public string ExternalApiClientId { get; set; } = string.Empty;
    public string ExternalApiClientSecret { get; set; } = string.Empty;
    public string ExternalApiTenantId { get; set; } = string.Empty;
    public string ExternalApiAuthorityUrl { get; set; } = string.Empty;
    public string CacheKeyExternalApi { get; set; } = string.Empty;
    public string ManagedIdentityClientId { get; set; } = string.Empty;
}
