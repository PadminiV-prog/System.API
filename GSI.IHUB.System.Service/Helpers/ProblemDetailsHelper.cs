using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Net;

namespace GSI.IHUB.System.Service.Helpers;

/// <summary>
/// Factory methods that create RFC 7807 <see cref="ProblemDetails"/> responses
/// with a consistent shape across the entire API surface.
/// </summary>
public static class ProblemDetailsHelper
{
    private const string BadRequestType          = "https://tools.ietf.org/html/rfc9110#section-15.5.1";
    private const string InternalServerErrorType = "https://tools.ietf.org/html/rfc9110#section-15.6.1";
    private const string ProblemDetailsContentType = "application/problem+json";

    /// <summary>
    /// Creates a 400 Bad Request <see cref="ProblemDetails"/> result.
    /// </summary>
    /// <param name="detail">Human-readable explanation of why the request was rejected.</param>
    /// <param name="instance">URI that identifies the specific request instance (e.g. the route).</param>
    /// <param name="correlationId">Correlation ID propagated from the request header.</param>
    public static ObjectResult ValidationError(string detail, string instance, string correlationId)
    {
        var problem = Build(
            type: BadRequestType,
            title: "Bad Request",
            status: (int)HttpStatusCode.BadRequest,
            detail: detail,
            instance: instance,
            correlationId: correlationId);

        return ToObjectResult(problem);
    }

    /// <summary>
    /// Creates a 500 Internal Server Error <see cref="ProblemDetails"/> result.
    /// </summary>
    /// <param name="detail">Human-readable explanation of the error.</param>
    /// <param name="instance">URI that identifies the specific request instance (e.g. the route).</param>
    /// <param name="correlationId">Correlation ID propagated from the request header.</param>
    public static ObjectResult InternalServerError(string detail, string instance, string correlationId)
    {
        var problem = Build(
            type: InternalServerErrorType,
            title: "Internal Server Error",
            status: (int)HttpStatusCode.InternalServerError,
            detail: detail,
            instance: instance,
            correlationId: correlationId);

        return ToObjectResult(problem);
    }

    // ── private helpers ──────────────────────────────────────────────────────

    private static ProblemDetails Build(
        string type,
        string title,
        int status,
        string detail,
        string instance,
        string correlationId)
    {
        var problem = new ProblemDetails
        {
            Type     = type,
            Title    = title,
            Status   = status,
            Detail   = detail,
            Instance = instance
        };

        problem.Extensions["correlationId"] = correlationId;
        return problem;
    }

    private static ObjectResult ToObjectResult(ProblemDetails problem) =>
        new(problem)
        {
            StatusCode   = problem.Status,
            ContentTypes = new MediaTypeCollection { ProblemDetailsContentType }
        };
}
