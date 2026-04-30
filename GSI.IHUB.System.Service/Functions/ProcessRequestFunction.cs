using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using GSI.IHUB.System.Service.Contracts;
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
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(string), Description = "Invalid request")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(string), Description = "Unhandled error")]
    [OpenApiResponseWithBody(HttpStatusCode.Unauthorized, "application/json", typeof(string), Description = "Unauthorized")]
    [OpenApiResponseWithBody(HttpStatusCode.Forbidden, "application/json", typeof(string), Description = "Forbidden")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "system/process")] HttpRequestData req,
        FunctionContext context)
    {
        var correlationId = context.Items.TryGetValue(ApplicationConstants.CorrelationIdHeaderKey, out var correlation)
            ? correlation?.ToString() ?? Guid.NewGuid().ToString()
            : Guid.NewGuid().ToString();

        _logger.LogInformation("ProcessRequest invoked. CorrelationId: {CorrelationId}", correlationId);

        try
        {
            using var reader = new StreamReader(req.Body);
            var body = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(body))
            {
                return new BadRequestObjectResult("Request body is required.");
            }

            var request = JsonConvert.DeserializeObject<SystemRequest>(body);
            if (request is null || !await _validationService.ValidateRequestAsync(request))
            {
                return new BadRequestObjectResult("Invalid request payload.");
            }

            var response = await _sysService.ProcessRequestAsync(request, correlationId);
            return new OkObjectResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing request. CorrelationId: {CorrelationId}", correlationId);
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }
}
