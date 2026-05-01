using Microsoft.AspNetCore.Mvc;
using Moq;
using GSI.IHUB.System.Service.Functions;
using GSI.IHUB.System.Service.Model;
using GSI.IHUB.System.Service.Tests.Setup;

namespace GSI.IHUB.System.Service.Tests.Functions;

public class ProcessRequestFunctionTests
{
    private readonly TestFixture _fixture = new();

    [Fact]
    public async Task ProcessRequest_ValidRequest_ReturnsOk()
    {
        var context = _fixture.CreateFunctionContext();
        var body = "{\"requestId\":\"123\",\"payload\":\"data\"}";
        var req = _fixture.CreateHttpRequestData(context, body);
        var systemRequest = new SystemRequest { RequestId = "123", Payload = "data" };

        _fixture.ValidationServiceMock
            .Setup(x => x.ValidateRequest(It.IsAny<string?>(), It.IsAny<string>()))
            .Returns((true, systemRequest));

        _fixture.SysServiceMock
            .Setup(x => x.ProcessRequestAsync(It.IsAny<SystemRequest>(), It.IsAny<string>()))
            .ReturnsAsync("{\"status\":\"ok\"}");

        var function = new ProcessRequestFunction(
            _fixture.LoggerMock.Object,
            _fixture.ValidationServiceMock.Object,
            _fixture.SysServiceMock.Object);

        var result = await function.Run(req, context);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("{\"status\":\"ok\"}", okResult.Value);
    }

    [Fact]
    public async Task ProcessRequest_EmptyBody_ReturnsBadRequestProblemDetails()
    {
        var context = _fixture.CreateFunctionContext();
        var req = _fixture.CreateHttpRequestData(context, string.Empty);
        SystemRequest? nullRequest = null;

        _fixture.ValidationServiceMock
            .Setup(x => x.ValidateRequest(It.IsAny<string?>(), It.IsAny<string>()))
            .Returns((false, nullRequest));

        var function = new ProcessRequestFunction(
            _fixture.LoggerMock.Object,
            _fixture.ValidationServiceMock.Object,
            _fixture.SysServiceMock.Object);

        var result = await function.Run(req, context);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(400, problem.Status);
        Assert.NotNull(problem.Extensions["correlationId"]);
    }

    [Fact]
    public async Task ProcessRequest_InvalidPayload_ReturnsBadRequestProblemDetails()
    {
        var context = _fixture.CreateFunctionContext();
        var req = _fixture.CreateHttpRequestData(context, "{\"requestId\":\"\",\"payload\":\"\"}");
        SystemRequest? nullRequest = null;

        _fixture.ValidationServiceMock
            .Setup(x => x.ValidateRequest(It.IsAny<string?>(), It.IsAny<string>()))
            .Returns((false, nullRequest));

        var function = new ProcessRequestFunction(
            _fixture.LoggerMock.Object,
            _fixture.ValidationServiceMock.Object,
            _fixture.SysServiceMock.Object);

        var result = await function.Run(req, context);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Bad Request", problem.Title);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.1", problem.Type);
    }

    [Fact]
    public async Task ProcessRequest_ServiceThrowsException_ReturnsInternalServerErrorProblemDetails()
    {
        var context = _fixture.CreateFunctionContext();
        var req = _fixture.CreateHttpRequestData(context, "{\"requestId\":\"123\",\"payload\":\"data\"}");
        var systemRequest = new SystemRequest { RequestId = "123", Payload = "data" };

        _fixture.ValidationServiceMock
            .Setup(x => x.ValidateRequest(It.IsAny<string?>(), It.IsAny<string>()))
            .Returns((true, systemRequest));

        _fixture.SysServiceMock
            .Setup(x => x.ProcessRequestAsync(It.IsAny<SystemRequest>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("failure"));

        var function = new ProcessRequestFunction(
            _fixture.LoggerMock.Object,
            _fixture.ValidationServiceMock.Object,
            _fixture.SysServiceMock.Object);

        var result = await function.Run(req, context);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(500, problem.Status);
        Assert.Equal("Internal Server Error", problem.Title);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.6.1", problem.Type);
        Assert.NotNull(problem.Extensions["correlationId"]);
    }

    [Fact]
    public async Task ProcessRequest_ValidationError_IncludesCorrelationIdInProblemDetails()
    {
        const string expectedCorrelationId = "test-correlation";
        var context = _fixture.CreateFunctionContext(expectedCorrelationId);
        var req = _fixture.CreateHttpRequestData(context, string.Empty);
        SystemRequest? nullRequest = null;

        _fixture.ValidationServiceMock
            .Setup(x => x.ValidateRequest(It.IsAny<string?>(), It.IsAny<string>()))
            .Returns((false, nullRequest));

        var function = new ProcessRequestFunction(
            _fixture.LoggerMock.Object,
            _fixture.ValidationServiceMock.Object,
            _fixture.SysServiceMock.Object);

        var result = await function.Run(req, context);

        var objectResult = Assert.IsType<ObjectResult>(result);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(expectedCorrelationId, problem.Extensions["correlationId"]?.ToString());
    }

    [Fact]
    public async Task ProcessRequest_InternalServerError_IncludesCorrelationIdInProblemDetails()
    {
        const string expectedCorrelationId = "test-correlation";
        var context = _fixture.CreateFunctionContext(expectedCorrelationId);
        var req = _fixture.CreateHttpRequestData(context, "{\"requestId\":\"123\",\"payload\":\"data\"}");
        var systemRequest = new SystemRequest { RequestId = "123", Payload = "data" };

        _fixture.ValidationServiceMock
            .Setup(x => x.ValidateRequest(It.IsAny<string?>(), It.IsAny<string>()))
            .Returns((true, systemRequest));

        _fixture.SysServiceMock
            .Setup(x => x.ProcessRequestAsync(It.IsAny<SystemRequest>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("downstream failure"));

        var function = new ProcessRequestFunction(
            _fixture.LoggerMock.Object,
            _fixture.ValidationServiceMock.Object,
            _fixture.SysServiceMock.Object);

        var result = await function.Run(req, context);

        var objectResult = Assert.IsType<ObjectResult>(result);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(expectedCorrelationId, problem.Extensions["correlationId"]?.ToString());
    }
}
