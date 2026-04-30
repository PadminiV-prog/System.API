using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using System.Text;
using GSI.IHUB.System.Service.Contracts;
using GSI.IHUB.System.Service.Model;

namespace GSI.IHUB.System.Service.Helpers;

public class HttpClientService : IHttpClientService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly AppSettings _appSettings;
    private readonly ILogger<HttpClientService> _logger;

    public HttpClientService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        AppSettings appSettings,
        ILogger<HttpClientService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _appSettings = appSettings;
        _logger = logger;
    }

    public async Task<string> PostAsync(ExternalApiRequestModel model, string correlationId)
    {
        var client = _httpClientFactory.CreateClient(ApplicationConstants.HttpClientName);
        var endpoint = BuildEndpoint(model);
        var token = await GetTokenAsync(model);

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(model.RequestData, Encoding.UTF8, "application/json")
        };

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        request.Headers.TryAddWithoutValidation(ApplicationConstants.CorrelationIdHeaderKey, correlationId);

        if (!string.IsNullOrWhiteSpace(model.SubscriptionKey))
        {
            request.Headers.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", model.SubscriptionKey);
        }

        _logger.LogInformation("Posting request to external API. CorrelationId: {CorrelationId}", correlationId);

        var response = await client.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        _logger.LogInformation(
            "External API response received. CorrelationId: {CorrelationId}, StatusCode: {StatusCode}",
            correlationId,
            response.StatusCode);

        return responseContent;
    }

    private async Task<string> GetTokenAsync(ExternalApiRequestModel model)
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

        var scope = string.IsNullOrWhiteSpace(model.Scope)
            ? BuildDefaultScope(model.BaseUrl)
            : model.Scope;

        var tokenResult = await app.AcquireTokenForClient(new[] { scope }).ExecuteAsync();

        _memoryCache.Set(
            model.CacheKey,
            tokenResult.AccessToken,
            tokenResult.ExpiresOn - DateTimeOffset.UtcNow - TimeSpan.FromMinutes(5));

        return tokenResult.AccessToken;
    }

    private static string BuildDefaultScope(string baseUrl)
    {
        return string.IsNullOrWhiteSpace(baseUrl)
            ? "https://management.azure.com/.default"
            : $"{baseUrl.TrimEnd('/')}/.default";
    }

    private static string BuildEndpoint(ExternalApiRequestModel model)
    {
        var baseUrl = model.BaseUrl.TrimEnd('/');

        if (string.IsNullOrWhiteSpace(model.ApiVersion))
        {
            return baseUrl;
        }

        var separator = baseUrl.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        return $"{baseUrl}{separator}api-version={Uri.EscapeDataString(model.ApiVersion)}";
    }
}
