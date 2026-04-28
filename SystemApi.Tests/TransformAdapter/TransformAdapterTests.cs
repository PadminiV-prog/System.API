using SystemApi;
using SystemApi.TransformAdapter;

namespace SystemApi.Tests.TransformAdapter;

public class TransformAdapterTests
{
    private readonly AppSettings _appSettings = new()
    {
        ExternalApiSubscriptionKey = "test-api-key",
        ExternalApiBaseUrl = "https://api.example.com"
    };

    [Fact]
    public void GetExternalAPIKeys_ReturnsSubscriptionKeyAndBaseUrl()
    {
        var adapter = new SystemApi.TransformAdapter.TransformAdapter(_appSettings);

        var (apiKey, baseUrl) = adapter.GetExternalAPIKeys();

        Assert.Equal("test-api-key", apiKey);
        Assert.Equal("https://api.example.com", baseUrl);
    }

    [Fact]
    public async Task ProcessRequestAsync_ReturnsSuccessMessage()
    {
        var adapter = new SystemApi.TransformAdapter.TransformAdapter(_appSettings);

        var result = await adapter.ProcessRequestAsync("{\"data\":\"test\"}", "test-api-key", "https://api.example.com", "test-correlation");

        Assert.False(string.IsNullOrWhiteSpace(result));
    }
}
