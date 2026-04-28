using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SystemApi.Model;

namespace SystemApi.Middleware;

public class RequestHeaderMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var loggerFactory = context.InstanceServices.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger<RequestHeaderMiddleware>();

        var request = await context.GetHttpRequestDataAsync();

        var correlationId = TryGetHeaderValue(request, ApplicationConstants.CorrelationIdHeaderKey) ?? Guid.NewGuid().ToString();

        context.Items[ApplicationConstants.CorrelationIdHeaderKey] = correlationId;

        logger?.LogInformation(
            "Request headers resolved. CorrelationId: {CorrelationId}",
            correlationId);

        await next(context);
    }

    private static string? TryGetHeaderValue(Microsoft.Azure.Functions.Worker.Http.HttpRequestData? request, string headerKey)
    {
        if (request?.Headers.TryGetValues(headerKey, out var values) == true)
        {
            return values.FirstOrDefault();
        }

        return null;
    }
}
