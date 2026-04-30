using Microsoft.Extensions.Logging;
using Moq;
using GSI.IHUB.System.Service.Contracts;
using GSI.IHUB.System.Service.Model;
using GSI.IHUB.System.Service.ServiceImplementation;

namespace GSI.IHUB.System.Service.Tests.ServiceImplementation;

public class SysServiceTests
{
    private readonly Mock<ILogger<SysService>> _loggerMock = new();
    private readonly Mock<IHttpClientService> _httpClientServiceMock = new();
    private readonly Mock<ITransformAdapterFactory> _transformAdapterFactoryMock = new();
    private readonly Mock<ITransformAdapter> _transformAdapterMock = new();

    private readonly AppSettings _appSettings = new()
    {
        ExternalApiBaseUrl = "https://api.example.com",
        ExternalApiSubscriptionKey = "test-key",
        ExternalApiClientId = "client-id",
        ExternalApiClientSecret = "client-secret",
        ExternalApiAuthorityUrl = "https://login.example.com",
        ExternalApiVersion = "v1",
        CacheKeyExternalApi = "TestCacheKey"
    };

    private SysService CreateSut() =>
        new(_loggerMock.Object, _appSettings, _httpClientServiceMock.Object, _transformAdapterFactoryMock.Object);

    [Fact]
    public async Task ProcessRequestAsync_ValidRequest_ReturnsHttpClientResponse()
    {
        var request = new SystemRequest { RequestId = "req-1", Payload = "payload-data" };
        const string correlationId = "corr-100";
        const string expectedResponse = "{\"status\":\"ok\"}";

        _transformAdapterMock
            .Setup(x => x.GetExternalAPIKeys())
            .Returns(("test-key", "https://api.example.com"));

        _transformAdapterFactoryMock
            .Setup(x => x.GetAdapter())
            .Returns(_transformAdapterMock.Object);

        _httpClientServiceMock
            .Setup(x => x.PostAsync(It.IsAny<ExternalApiRequestModel>(), correlationId))
            .ReturnsAsync(expectedResponse);

        var sut = CreateSut();
        var result = await sut.ProcessRequestAsync(request, correlationId);

        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task ProcessRequestAsync_NoAdapterAvailable_ReturnsNoAdapterMessage()
    {
        var request = new SystemRequest { RequestId = "req-1", Payload = "payload-data" };
        const string correlationId = "corr-101";

        _transformAdapterFactoryMock
            .Setup(x => x.GetAdapter())
            .Returns((ITransformAdapter?)null!);

        var sut = CreateSut();
        var result = await sut.ProcessRequestAsync(request, correlationId);

        Assert.Equal("No adapter found.", result);
    }

    [Fact]
    public async Task ProcessRequestAsync_EmptyHttpClientResponse_ReturnsNoResponse()
    {
        var request = new SystemRequest { RequestId = "req-1", Payload = "payload-data" };
        const string correlationId = "corr-102";

        _transformAdapterMock
            .Setup(x => x.GetExternalAPIKeys())
            .Returns(("test-key", "https://api.example.com"));

        _transformAdapterFactoryMock
            .Setup(x => x.GetAdapter())
            .Returns(_transformAdapterMock.Object);

        _httpClientServiceMock
            .Setup(x => x.PostAsync(It.IsAny<ExternalApiRequestModel>(), correlationId))
            .ReturnsAsync(string.Empty);

        var sut = CreateSut();
        var result = await sut.ProcessRequestAsync(request, correlationId);

        Assert.Equal("NoResponse", result);
    }

    [Fact]
    public async Task ProcessRequestAsync_HttpClientThrows_RethrowsException()
    {
        var request = new SystemRequest { RequestId = "req-1", Payload = "payload-data" };
        const string correlationId = "corr-103";

        _transformAdapterMock
            .Setup(x => x.GetExternalAPIKeys())
            .Returns(("test-key", "https://api.example.com"));

        _transformAdapterFactoryMock
            .Setup(x => x.GetAdapter())
            .Returns(_transformAdapterMock.Object);

        _httpClientServiceMock
            .Setup(x => x.PostAsync(It.IsAny<ExternalApiRequestModel>(), correlationId))
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var sut = CreateSut();

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            sut.ProcessRequestAsync(request, correlationId));
    }

    [Fact]
    public async Task ProcessRequestAsync_PostAsyncCalledWithCorrectCorrelationId()
    {
        var request = new SystemRequest { RequestId = "req-1", Payload = "payload-data" };
        const string correlationId = "corr-104";

        _transformAdapterMock
            .Setup(x => x.GetExternalAPIKeys())
            .Returns(("test-key", "https://api.example.com"));

        _transformAdapterFactoryMock
            .Setup(x => x.GetAdapter())
            .Returns(_transformAdapterMock.Object);

        _httpClientServiceMock
            .Setup(x => x.PostAsync(It.IsAny<ExternalApiRequestModel>(), It.IsAny<string>()))
            .ReturnsAsync("ok");

        var sut = CreateSut();
        await sut.ProcessRequestAsync(request, correlationId);

        _httpClientServiceMock.Verify(
            x => x.PostAsync(
                It.Is<ExternalApiRequestModel>(m => m.CorrelationId == correlationId),
                correlationId),
            Times.Once);
    }

    [Fact]
    public async Task ProcessRequestAsync_PostAsyncCalledWithSerializedRequest()
    {
        var request = new SystemRequest { RequestId = "req-5", Payload = "payload-5" };
        const string correlationId = "corr-105";

        _transformAdapterMock
            .Setup(x => x.GetExternalAPIKeys())
            .Returns(("key", "https://api.example.com"));

        _transformAdapterFactoryMock
            .Setup(x => x.GetAdapter())
            .Returns(_transformAdapterMock.Object);

        _httpClientServiceMock
            .Setup(x => x.PostAsync(It.IsAny<ExternalApiRequestModel>(), It.IsAny<string>()))
            .ReturnsAsync("ok");

        var sut = CreateSut();
        await sut.ProcessRequestAsync(request, correlationId);

        _httpClientServiceMock.Verify(
            x => x.PostAsync(
                It.Is<ExternalApiRequestModel>(m =>
                    m.RequestData.Contains("req-5") && m.RequestData.Contains("payload-5")),
                correlationId),
            Times.Once);
    }
}
