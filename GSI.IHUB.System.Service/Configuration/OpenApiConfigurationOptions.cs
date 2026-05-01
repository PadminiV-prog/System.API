using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace GSI.IHUB.System.Service.Configuration;

/// <summary>
/// Configures the OpenAPI/Swagger document metadata for this API.
/// Registered in the DI container so the
/// <c>Microsoft.Azure.Functions.Worker.Extensions.OpenApi</c> package
/// picks it up automatically to generate the Swagger UI and spec.
/// </summary>
public class OpenApiConfigurationOptions : DefaultOpenApiConfigurationOptions
{
    /// <inheritdoc />
    public override OpenApiInfo Info { get; set; } = new()
    {
        Version     = "v1.0.0",
        Title       = "GSI IHUB System API",
        Description = "Pass-through API that integrates with external systems via a secure, observable, and resilient HTTP pipeline.",
        Contact     = new OpenApiContact
        {
            Name = "GSI IHUB Team"
        }
    };

    /// <inheritdoc />
    /// <remarks>Promotes the document to OpenAPI 3.0 (V3).</remarks>
    public override OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V3;
}
