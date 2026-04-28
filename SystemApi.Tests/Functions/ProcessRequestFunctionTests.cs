using Microsoft.AspNetCore.Mvc;
using Moq;
using SystemApi.Functions;
using SystemApi.Model;
using SystemApi.Tests.Setup;

namespace SystemApi.Tests.Functions;

public class ProcessRequestFunctionTests
{
    private readonly TestFixture _fixture = new();

    [Fact]
    public async Task ProcessRequest_ValidRequest_ReturnsOk()
    {
        var context = _fixture.CreateFunctionContext();
        var request = _fixture.CreateHttpRequestData(context, "{\"requestId\":\"123\",\"payload\":\"data\"}");

        _fixture.ValidationServiceMock
            .Setup(x => x.ValidateRequestAsync(It.IsAny<SystemRequest>()))
            .ReturnsAsync(true);

        _fixture.SysServiceMock
            .Setup(x => x.ProcessRequestAsync(It.IsAny<SystemRequest>(), "test-correlation"))
            .ReturnsAsync("{\"status\":\"ok\"}");

        var function = new ProcessRequestFunction(
            _fixture.LoggerMock.Object,
            _fixture.ValidationServiceMock.Object,
            _fixture.SysServiceMock.Object);

        var result = await function.Run(request, context);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("{\"status\":\"ok\"}", okResult.Value);
    }

    [Fact]
    public async Task ProcessRequest_EmptyBody_ReturnsBadRequest()
    {
        var context = _fixture.CreateFunctionContext();
        var request = _fixture.CreateHttpRequestData(context, string.Empty);

        var function = new ProcessRequestFunction(
            _fixture.LoggerMock.Object,
            _fixture.ValidationServiceMock.Object,
            _fixture.SysServiceMock.Object);

        var result = await function.Run(request, context);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ProcessRequest_ServiceThrowsException_ReturnsInternalServerError()
    {
        var context = _fixture.CreateFunctionContext();
        var request = _fixture.CreateHttpRequestData(context, "{\"requestId\":\"123\",\"payload\":\"data\"}");

        _fixture.ValidationServiceMock
            .Setup(x => x.ValidateRequestAsync(It.IsAny<SystemRequest>()))
            .ReturnsAsync(true);

        _fixture.SysServiceMock
            .Setup(x => x.ProcessRequestAsync(It.IsAny<SystemRequest>(), "test-correlation"))
            .ThrowsAsync(new Exception("failure"));

        var function = new ProcessRequestFunction(
            _fixture.LoggerMock.Object,
            _fixture.ValidationServiceMock.Object,
            _fixture.SysServiceMock.Object);

        var result = await function.Run(request, context);

        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }
}
