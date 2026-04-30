namespace GSI.IHUB.System.Service.Model;

public static class ApplicationConstants
{
    public const string CorrelationIdHeaderKey = "x-correlation-id";
    public const string CacheKeyExternalApi = "ExternalApiToken";
    public const string HttpClientName = "SystemApiHttpClient";

    // Polly retry defaults (used when config values are absent)
    public const int MaxRetryCount = 3;
    public const int PollyRetryInterval = 2;
}
