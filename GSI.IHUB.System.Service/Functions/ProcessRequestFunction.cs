using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using System.Net;
using GSI.IHUB.System.Service.Contracts;
using GSI.IHUB.System.Service.Helpers;
using GSI.IHUB.System.Service.Model;

namespace GSI.IHUB.System.Service.Functions;

public class ProcessRequestFunction
{
    private readonly ILogger<ProcessRequestFunction> _logger;
    private readonly IValidationService _validationService;
    private readonly ISysService _sysService;

    public ProcessRequestFunction(
        ILogger<ProcessRequestFunction> logger,
        IValidationService validationService,
        ISysService sysService)
    {
        _logger = logger;
        _validationService = validationService;
        _sysService = sysService;
    }

    [Function("ProcessRequest")]
    [OpenApiOperation(operationId: "ProcessRequest", tags: new[] { "SystemApi" }, Description = "Pass-through endpoint for external API integration")]
    [OpenApiSecurity("BearerAuth", Microsoft.OpenApi.Models.SecuritySchemeType.Http, BearerFormat = "JWT", Scheme = OpenApiSecuritySchemeType.Bearer)]
    [OpenApiRequestBody("application/json", typeof(SystemRequest), Required = true, Description = "System API request payload")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(string), Description = "Processed response")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/problem+json", typeof(ProblemDetails), Description = "Invalid request")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/problem+json", typeof(ProblemDetails), Description = "Unhandled error")]
    [OpenApiResponseWithBody(HttpStatusCode.Unauthorized, "application/problem+json", typeof(ProblemDetails), Description = "Unauthorized")]
    [OpenApiResponseWithBody(HttpStatusCode.Forbidden, "application/problem+json", typeof(ProblemDetails), Description = "Forbidden")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "system/process")] HttpRequestData req,
        FunctionContext context)
    {
        var correlationId = context.Items.TryGetValue(ApplicationConstants.CorrelationIdHeaderKey, out var correlation)
            ? correlation?.ToString() ?? Guid.NewGuid().ToString()
            : Guid.NewGuid().ToString();

        var instance = req.Url.PathAndQuery;

        _logger.LogInformation(
            "ProcessRequest invoked. CorrelationId: {CorrelationId}",
            correlationId);

        try
        {
            using var reader = new StreamReader(req.Body);
            var body = await reader.ReadToEndAsync();

            var (isValid, request) = _validationService.ValidateRequest(body, correlationId);
            if (!isValid || request is null)
            {
                _logger.LogWarning(
                    "ProcessRequest rejected: validation failed. CorrelationId: {CorrelationId}",
                    correlationId);
                return ProblemDetailsHelper.ValidationError(
                    detail: "The request body is missing, empty, or contains invalid/incomplete data.",
                    instance: instance,
                    correlationId: correlationId);
            }

            var response = await _sysService.ProcessRequestAsync(request, correlationId);

            _logger.LogInformation(
                "ProcessRequest completed successfully. CorrelationId: {CorrelationId}",
                correlationId);
            return new OkObjectResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception while processing request. CorrelationId: {CorrelationId}",
                correlationId);
            return ProblemDetailsHelper.InternalServerError(
                detail: "An unexpected error occurred while processing the request.",
                instance: instance,
                correlationId: correlationId);
        }
    }
}
