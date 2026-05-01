using Microsoft.AspNetCore.Mvc;
using GSI.IHUB.System.Service.Helpers;

namespace GSI.IHUB.System.Service.Tests.Helpers;

public class ProblemDetailsHelperTests
{
    // ──────────────────────────────────────────────────────────
    // ValidationError — 400 Bad Request
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void ValidationError_ReturnsObjectResultWithStatus400()
    {
        var result = ProblemDetailsHelper.ValidationError("bad input", "/api/test", "corr-1");

        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public void ValidationError_ValueIsProblemDetails()
    {
        var result = ProblemDetailsHelper.ValidationError("bad input", "/api/test", "corr-2");

        Assert.IsType<ProblemDetails>(result.Value);
    }

    [Fact]
    public void ValidationError_ProblemDetailsHasCorrectStatus()
    {
        var result = ProblemDetailsHelper.ValidationError("bad input", "/api/test", "corr-3");
        var problem = (ProblemDetails)result.Value!;

        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public void ValidationError_ProblemDetailsHasCorrectTitle()
    {
        var result = ProblemDetailsHelper.ValidationError("bad input", "/api/test", "corr-4");
        var problem = (ProblemDetails)result.Value!;

        Assert.Equal("Bad Request", problem.Title);
    }

    [Fact]
    public void ValidationError_ProblemDetailsHasCorrectType()
    {
        var result = ProblemDetailsHelper.ValidationError("bad input", "/api/test", "corr-5");
        var problem = (ProblemDetails)result.Value!;

        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.1", problem.Type);
    }

    [Fact]
    public void ValidationError_ProblemDetailsHasCorrectDetail()
    {
        const string detail = "Request body is missing required fields.";
        var result = ProblemDetailsHelper.ValidationError(detail, "/api/test", "corr-6");
        var problem = (ProblemDetails)result.Value!;

        Assert.Equal(detail, problem.Detail);
    }

    [Fact]
    public void ValidationError_ProblemDetailsHasCorrectInstance()
    {
        const string instance = "/system/process";
        var result = ProblemDetailsHelper.ValidationError("bad input", instance, "corr-7");
        var problem = (ProblemDetails)result.Value!;

        Assert.Equal(instance, problem.Instance);
    }

    [Fact]
    public void ValidationError_ProblemDetailsContainsCorrelationId()
    {
        const string correlationId = "corr-8";
        var result = ProblemDetailsHelper.ValidationError("bad input", "/api/test", correlationId);
        var problem = (ProblemDetails)result.Value!;

        Assert.Equal(correlationId, problem.Extensions["correlationId"]?.ToString());
    }

    [Fact]
    public void ValidationError_ContentTypeIsApplicationProblemJson()
    {
        var result = ProblemDetailsHelper.ValidationError("bad input", "/api/test", "corr-9");

        Assert.Contains("application/problem+json", result.ContentTypes);
    }

    // ──────────────────────────────────────────────────────────
    // InternalServerError — 500
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void InternalServerError_ReturnsObjectResultWithStatus500()
    {
        var result = ProblemDetailsHelper.InternalServerError("unexpected error", "/api/test", "corr-10");

        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public void InternalServerError_ValueIsProblemDetails()
    {
        var result = ProblemDetailsHelper.InternalServerError("unexpected error", "/api/test", "corr-11");

        Assert.IsType<ProblemDetails>(result.Value);
    }

    [Fact]
    public void InternalServerError_ProblemDetailsHasCorrectStatus()
    {
        var result = ProblemDetailsHelper.InternalServerError("unexpected error", "/api/test", "corr-12");
        var problem = (ProblemDetails)result.Value!;

        Assert.Equal(500, problem.Status);
    }

    [Fact]
    public void InternalServerError_ProblemDetailsHasCorrectTitle()
    {
        var result = ProblemDetailsHelper.InternalServerError("unexpected error", "/api/test", "corr-13");
        var problem = (ProblemDetails)result.Value!;

        Assert.Equal("Internal Server Error", problem.Title);
    }

    [Fact]
    public void InternalServerError_ProblemDetailsHasCorrectType()
    {
        var result = ProblemDetailsHelper.InternalServerError("unexpected error", "/api/test", "corr-14");
        var problem = (ProblemDetails)result.Value!;

        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.6.1", problem.Type);
    }

    [Fact]
    public void InternalServerError_ProblemDetailsHasCorrectDetail()
    {
        const string detail = "An unexpected error occurred.";
        var result = ProblemDetailsHelper.InternalServerError(detail, "/api/test", "corr-15");
        var problem = (ProblemDetails)result.Value!;

        Assert.Equal(detail, problem.Detail);
    }

    [Fact]
    public void InternalServerError_ProblemDetailsHasCorrectInstance()
    {
        const string instance = "/system/process";
        var result = ProblemDetailsHelper.InternalServerError("unexpected error", instance, "corr-16");
        var problem = (ProblemDetails)result.Value!;

        Assert.Equal(instance, problem.Instance);
    }

    [Fact]
    public void InternalServerError_ProblemDetailsContainsCorrelationId()
    {
        const string correlationId = "corr-17";
        var result = ProblemDetailsHelper.InternalServerError("unexpected error", "/api/test", correlationId);
        var problem = (ProblemDetails)result.Value!;

        Assert.Equal(correlationId, problem.Extensions["correlationId"]?.ToString());
    }

    [Fact]
    public void InternalServerError_ContentTypeIsApplicationProblemJson()
    {
        var result = ProblemDetailsHelper.InternalServerError("unexpected error", "/api/test", "corr-18");

        Assert.Contains("application/problem+json", result.ContentTypes);
    }
}
