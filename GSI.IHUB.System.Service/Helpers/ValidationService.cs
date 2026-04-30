using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using GSI.IHUB.System.Service.Contracts;
using GSI.IHUB.System.Service.Model;

namespace GSI.IHUB.System.Service.Helpers;

public class ValidationService : IValidationService
{
    private readonly ILogger<ValidationService> _logger;

    public ValidationService(ILogger<ValidationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Validates the raw request body: rejects null/whitespace, empty JSON objects,
    /// and payloads that cannot be deserialised or are missing required fields.
    /// </summary>
    public (bool IsValid, SystemRequest? Request) ValidateRequest(string? body, string correlationId)
    {
        if (!IsValidRequest(body))
        {
            _logger.LogWarning(
                "Validation failed: request body is empty, whitespace, or an empty JSON object. CorrelationId: {CorrelationId}",
                correlationId);
            return (false, null);
        }

        SystemRequest? request;
        try
        {
            request = JsonConvert.DeserializeObject<SystemRequest>(body!);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Validation failed: unable to deserialize request body. CorrelationId: {CorrelationId}",
                correlationId);
            return (false, null);
        }

        if (request is null
            || string.IsNullOrWhiteSpace(request.RequestId)
            || string.IsNullOrWhiteSpace(request.Payload))
        {
            _logger.LogWarning(
                "Validation failed: required fields (RequestId, Payload) are missing or empty. CorrelationId: {CorrelationId}",
                correlationId);
            return (false, null);
        }

        _logger.LogInformation(
            "Request validation passed. CorrelationId: {CorrelationId}",
            correlationId);
        return (true, request);
    }

    /// <summary>
    /// Returns false for null/whitespace strings and for the literal empty-object JSON value "{}".
    /// </summary>
    public static bool IsValidRequest(string? request)
    {
        if (string.IsNullOrWhiteSpace(request) || request.Trim() == "{}")
        {
            return false;
        }

        return true;
    }
}
