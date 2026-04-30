using Microsoft.Extensions.Logging;
using Moq;
using GSI.IHUB.System.Service.Helpers;

namespace GSI.IHUB.System.Service.Tests.Helpers;

public class ValidationServiceTests
{
    private readonly ValidationService _sut;

    public ValidationServiceTests()
    {
        var logger = new Mock<ILogger<ValidationService>>();
        _sut = new ValidationService(logger.Object);
    }

    // ──────────────────────────────────────────────────────────
    // IsValidRequest static helper
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void IsValidRequest_NullString_ReturnsFalse()
    {
        Assert.False(ValidationService.IsValidRequest(null));
    }

    [Fact]
    public void IsValidRequest_EmptyString_ReturnsFalse()
    {
        Assert.False(ValidationService.IsValidRequest(string.Empty));
    }

    [Fact]
    public void IsValidRequest_WhiteSpaceString_ReturnsFalse()
    {
        Assert.False(ValidationService.IsValidRequest("   "));
    }

    [Fact]
    public void IsValidRequest_EmptyJsonObject_ReturnsFalse()
    {
        Assert.False(ValidationService.IsValidRequest("{}"));
    }

    [Fact]
    public void IsValidRequest_EmptyJsonObjectWithSpaces_ReturnsFalse()
    {
        Assert.False(ValidationService.IsValidRequest("  {}  "));
    }

    [Fact]
    public void IsValidRequest_ValidJsonString_ReturnsTrue()
    {
        Assert.True(ValidationService.IsValidRequest("{\"requestId\":\"1\",\"payload\":\"x\"}"));
    }

    // ──────────────────────────────────────────────────────────
    // ValidateRequest instance method
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void ValidateRequest_NullBody_ReturnsFalseAndNullRequest()
    {
        var (isValid, request) = _sut.ValidateRequest(null, "corr-001");

        Assert.False(isValid);
        Assert.Null(request);
    }

    [Fact]
    public void ValidateRequest_EmptyBody_ReturnsFalseAndNullRequest()
    {
        var (isValid, request) = _sut.ValidateRequest(string.Empty, "corr-002");

        Assert.False(isValid);
        Assert.Null(request);
    }

    [Fact]
    public void ValidateRequest_EmptyJsonObject_ReturnsFalseAndNullRequest()
    {
        var (isValid, request) = _sut.ValidateRequest("{}", "corr-003");

        Assert.False(isValid);
        Assert.Null(request);
    }

    [Fact]
    public void ValidateRequest_InvalidJson_ReturnsFalseAndNullRequest()
    {
        var (isValid, request) = _sut.ValidateRequest("not-valid-json{{", "corr-004");

        Assert.False(isValid);
        Assert.Null(request);
    }

    [Fact]
    public void ValidateRequest_ValidJson_ReturnsTrueAndDeserializedRequest()
    {
        var body = "{\"requestId\":\"req-1\",\"payload\":\"some-data\"}";

        var (isValid, request) = _sut.ValidateRequest(body, "corr-005");

        Assert.True(isValid);
        Assert.NotNull(request);
        Assert.Equal("req-1", request!.RequestId);
        Assert.Equal("some-data", request.Payload);
    }

    [Fact]
    public void ValidateRequest_MissingRequestId_ReturnsFalseAndNullRequest()
    {
        var body = "{\"requestId\":\"\",\"payload\":\"some-data\"}";

        var (isValid, request) = _sut.ValidateRequest(body, "corr-006");

        Assert.False(isValid);
        Assert.Null(request);
    }

    [Fact]
    public void ValidateRequest_MissingPayload_ReturnsFalseAndNullRequest()
    {
        var body = "{\"requestId\":\"req-1\",\"payload\":\"\"}";

        var (isValid, request) = _sut.ValidateRequest(body, "corr-007");

        Assert.False(isValid);
        Assert.Null(request);
    }

    [Fact]
    public void ValidateRequest_NullRequestIdInJson_ReturnsFalseAndNullRequest()
    {
        var body = "{\"requestId\":null,\"payload\":\"some-data\"}";

        var (isValid, request) = _sut.ValidateRequest(body, "corr-008");

        Assert.False(isValid);
        Assert.Null(request);
    }

    [Fact]
    public void ValidateRequest_WhiteSpaceBody_ReturnsFalseAndNullRequest()
    {
        var (isValid, request) = _sut.ValidateRequest("   ", "corr-009");

        Assert.False(isValid);
        Assert.Null(request);
    }
}
