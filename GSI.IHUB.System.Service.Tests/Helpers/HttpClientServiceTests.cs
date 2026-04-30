using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text;
using GSI.IHUB.System.Service.Helpers;
using GSI.IHUB.System.Service.Model;

namespace GSI.IHUB.System.Service.Tests.Helpers;

public class HttpClientServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<IMemoryCache> _memoryCacheMock = new();
    private readonly Mock<ILogger<HttpClientService>> _loggerMock = new();

    private readonly AppSettings _appSettings = new()
    {
        ExternalApiBaseUrl = "https://api.example.com",
        ExternalApiSubscriptionKey = "test-key",
        ExternalApiClientId = string.Empty,    // skip MSAL so no real network call needed
        ExternalApiClientSecret = string.Empty,
        ExternalApiAuthorityUrl = string.Empty,
        ExternalApiVersion = "v1",
        CacheKeyExternalApi = "TestToken"
    };

    private HttpClientService CreateSut(HttpClient? httpClient = null)
    {
        var client = httpClient ?? new HttpClient(new NoOpHttpMessageHandler());
        _httpClientFactoryMock
            .Setup(f => f.CreateClient(ApplicationConstants.HttpClientName))
            .Returns(client);

        return new HttpClientService(
            _httpClientFactoryMock.Object,
            _memoryCacheMock.Object,
            _appSettings,
            _loggerMock.Object);
    }

    // ──────────────────────────────────────────────────────────
    // PostAsync – happy path
    // ──────────────────────────────────────────────────────────

    [Fact]
    public async Task PostAsync_SuccessfulResponse_ReturnsResponseContent()
    {
        const string responseBody = "{\"result\":\"ok\"}";
        var httpClient = BuildHttpClient(HttpStatusCode.OK, responseBody);
        SetupNoCachedToken();

        var model = BuildModel();
        var sut = CreateSut(httpClient);

        var result = await sut.PostAsync(model, "corr-200");

        Assert.Equal(responseBody, result);
    }

    [Fact]
    public async Task PostAsync_NoClientIdConfigured_SkipsTokenAndSendsRequest()
    {
        const string responseBody = "success";
        var httpClient = BuildHttpClient(HttpStatusCode.OK, responseBody);
        SetupNoCachedToken();

        var model = BuildModel();      // appSettings has empty ClientId → no MSAL call
        var sut = CreateSut(httpClient);

        var result = await sut.PostAsync(model, "corr-201");

        Assert.Equal(responseBody, result);
    }

    [Fact]
    public async Task PostAsync_WithSubscriptionKey_AddsOcpHeaderToRequest()
    {
        var handler = new CapturingHttpMessageHandler(HttpStatusCode.OK, "{}");
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.com") };
        SetupNoCachedToken();

        var model = BuildModel(subscriptionKey: "ocp-key-123");
        var sut = CreateSut(httpClient);

        await sut.PostAsync(model, "corr-202");

        Assert.True(handler.LastRequest?.Headers.Contains("Ocp-Apim-Subscription-Key"));
    }

    [Fact]
    public async Task PostAsync_CorrelationIdPropagated_AddsCorrelationHeader()
    {
        const string correlationId = "corr-203";
        var handler = new CapturingHttpMessageHandler(HttpStatusCode.OK, "{}");
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.com") };
        SetupNoCachedToken();

        var model = BuildModel();
        var sut = CreateSut(httpClient);

        await sut.PostAsync(model, correlationId);

        Assert.True(handler.LastRequest?.Headers.Contains(ApplicationConstants.CorrelationIdHeaderKey));
    }

    [Fact]
    public async Task PostAsync_ApiVersionPresent_AppendsQueryParam()
    {
        var handler = new CapturingHttpMessageHandler(HttpStatusCode.OK, "{}");
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.com") };
        SetupNoCachedToken();

        var model = BuildModel(apiVersion: "2024-01-01");
        var sut = CreateSut(httpClient);

        await sut.PostAsync(model, "corr-204");

        Assert.Contains("api-version=", handler.LastRequest?.RequestUri?.Query ?? string.Empty);
    }

    // ──────────────────────────────────────────────────────────
    // Private helpers
    // ──────────────────────────────────────────────────────────

    private void SetupNoCachedToken()
    {
        object? ignored = null;
        _memoryCacheMock
            .Setup(c => c.TryGetValue(It.IsAny<object>(), out ignored))
            .Returns(false);
    }

    private static HttpClient BuildHttpClient(HttpStatusCode statusCode, string responseBody)
    {
        var handler = new NoOpHttpMessageHandler(statusCode, responseBody);
        return new HttpClient(handler) { BaseAddress = new Uri("https://api.example.com") };
    }

    private static ExternalApiRequestModel BuildModel(
        string? subscriptionKey = null,
        string? apiVersion = null) => new()
    {
        BaseUrl = "https://api.example.com",
        RequestData = "{\"requestId\":\"1\",\"payload\":\"x\"}",
        SubscriptionKey = subscriptionKey ?? string.Empty,
        ApiVersion = apiVersion ?? string.Empty,
        CacheKey = "TestToken",
        CorrelationId = "corr-test"
    };

    // ──────────────────────────────────────────────────────────
    // Test doubles
    // ──────────────────────────────────────────────────────────

    private sealed class NoOpHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _body;

        public NoOpHttpMessageHandler(
            HttpStatusCode statusCode = HttpStatusCode.OK,
            string body = "{}") =>
            (_statusCode, _body) = (statusCode, body);

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json")
            });
    }

    private sealed class CapturingHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _body;
        public HttpRequestMessage? LastRequest { get; private set; }

        public CapturingHttpMessageHandler(HttpStatusCode statusCode, string body) =>
            (_statusCode, _body) = (statusCode, body);

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json")
            });
        }
    }
}
