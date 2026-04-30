using GSI.IHUB.System.Service;
using GSI.IHUB.System.Service.Contracts;
using GSI.IHUB.System.Service.TransformFactory;

namespace GSI.IHUB.System.Service.Tests.TransformFactory;

public class TransformAdapterFactoryTests
{
    private readonly AppSettings _appSettings = new()
    {
        ExternalApiSubscriptionKey = "test-api-key",
        ExternalApiBaseUrl = "https://api.example.com"
    };

    [Fact]
    public void GetAdapter_ReturnsNonNullAdapter()
    {
        var factory = new TransformAdapterFactory(_appSettings);

        var adapter = factory.GetAdapter();

        Assert.NotNull(adapter);
    }

    [Fact]
    public void GetAdapter_ReturnsITransformAdapter()
    {
        var factory = new TransformAdapterFactory(_appSettings);

        var adapter = factory.GetAdapter();

        Assert.IsAssignableFrom<ITransformAdapter>(adapter);
    }

    [Fact]
    public void GetAdapter_ReturnsAdapterWithCorrectKeys()
    {
        var factory = new TransformAdapterFactory(_appSettings);

        var adapter = factory.GetAdapter();
        var (apiKey, baseUrl) = adapter.GetExternalAPIKeys();

        Assert.Equal("test-api-key", apiKey);
        Assert.Equal("https://api.example.com", baseUrl);
    }
}
