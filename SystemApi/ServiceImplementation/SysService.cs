using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using SystemApi.Contracts;
using SystemApi.Model;

namespace SystemApi.ServiceImplementation;

public class SysService : ISysService
{
    private readonly IHttpClientService _httpClientService;
    private readonly AppSettings _settings;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<SysService> _logger;

    public SysService(
        IHttpClientService httpClientService,
        AppSettings settings,
        IMemoryCache memoryCache,
        ILogger<SysService> logger)
    {
        _httpClientService = httpClientService;
        _settings = settings;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<string> ProcessRequestAsync(SystemRequest request, string correlationId, string sourceId)
    {
        _logger.LogInformation("Entering ProcessRequestAsync. CorrelationId: {CorrelationId}", correlationId);

        var model = new ExternalApiRequestModel
        {
            BaseUrl = _settings.ExternalApiBaseUrl,
            ClientId = _settings.ExternalApiClientId,
            ClientSecret = _settings.ExternalApiClientSecret,
            AuthorityUrl = _settings.ExternalApiAuthorityUrl,
            SubscriptionKey = _settings.ExternalApiSubscriptionKey,
            ApiVersion = _settings.ExternalApiVersion,
            CacheKey = string.IsNullOrWhiteSpace(_settings.CacheKeyExternalApi)
                ? ApplicationConstants.CacheKeyExternalApi
                : _settings.CacheKeyExternalApi,
            Scope = BuildDefaultScope(_settings.ExternalApiBaseUrl),
            RequestData = JsonConvert.SerializeObject(request),
            CorrelationId = correlationId,
            SourceId = sourceId
        };

        var token = await GetExternalApiTokenAsync(model);
        var result = await _httpClientService.PostAsync(model, token, correlationId, sourceId);

        return string.IsNullOrWhiteSpace(result) ? "NoResponse" : result;
    }

    private async Task<string> GetExternalApiTokenAsync(ExternalApiRequestModel model)
    {
        if (string.IsNullOrWhiteSpace(model.ClientId) ||
            string.IsNullOrWhiteSpace(model.ClientSecret) ||
            string.IsNullOrWhiteSpace(model.AuthorityUrl))
        {
            _logger.LogInformation("Skipping JWT token generation and using APIM key/auth fallback.");
            return string.Empty;
        }

        if (_memoryCache.TryGetValue<string>(model.CacheKey, out var cachedToken) && !string.IsNullOrWhiteSpace(cachedToken))
        {
            return cachedToken;
        }

        var app = ConfidentialClientApplicationBuilder
            .Create(model.ClientId)
            .WithClientSecret(model.ClientSecret)
            .WithAuthority(model.AuthorityUrl)
            .Build();

        var scope = string.IsNullOrWhiteSpace(model.Scope) ? BuildDefaultScope(model.BaseUrl) : model.Scope;
        var tokenResult = await app.AcquireTokenForClient(new[] { scope }).ExecuteAsync();

        _memoryCache.Set(
            model.CacheKey,
            tokenResult.AccessToken,
            tokenResult.ExpiresOn - DateTimeOffset.UtcNow - TimeSpan.FromMinutes(5));

        return tokenResult.AccessToken;
    }

    private static string BuildDefaultScope(string externalApiBaseUrl)
    {
        return string.IsNullOrWhiteSpace(externalApiBaseUrl)
            ? "https://management.azure.com/.default"
            : $"{externalApiBaseUrl.TrimEnd('/')}/.default";
    }
}
