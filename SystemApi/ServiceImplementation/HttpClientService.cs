using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using SystemApi.Contracts;
using SystemApi.Model;

namespace SystemApi.ServiceImplementation;

public class HttpClientService : IHttpClientService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpClientService> _logger;

    public HttpClientService(IHttpClientFactory httpClientFactory, ILogger<HttpClientService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string> PostAsync(ExternalApiRequestModel model, string token, string correlationId)
    {
        var client = _httpClientFactory.CreateClient(ApplicationConstants.HttpClientName);
        var endpoint = BuildEndpoint(model);

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
