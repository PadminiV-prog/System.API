namespace SystemApi.Model;

public class ExternalApiRequestModel
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string AuthorityUrl { get; set; } = string.Empty;
    public string SubscriptionKey { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = string.Empty;
    public string CacheKey { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string RequestData { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
}
