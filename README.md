# GSI IHUB System API

A pass-through Azure Functions API that validates incoming requests and relays them to an external system via a secure, observable, and resilient HTTP pipeline. It supports OAuth 2.0 client-credentials authentication, APIM subscription-key forwarding, Polly-based resilience, in-memory token caching, correlation-ID tracing, and an OpenAPI (Swagger) UI.

## Solution structure

```
GSI.IHUB.System.Service.sln
├── GSI.IHUB.System.Service/          # .NET 10 Azure Functions v4 isolated-worker project
│   ├── Program.cs                    # Host bootstrap + OpenApiConfigurationOptions class
│   ├── AppSettings.cs                # Strongly-typed configuration model
│   ├── Configuration/                # Per-environment JSON config files
│   │   ├── dev.json
│   │   ├── qa.json
│   │   ├── uat.json
│   │   ├── prd.json
│   │   ├── dr.json
│   │   ├── dvhf.json
│   │   ├── perf.json
│   │   └── uthf.json
│   ├── Contracts/                    # Service interfaces
│   ├── Functions/
│   │   └── ProcessRequestFunction.cs # POST /api/system/process trigger
│   ├── Helpers/
│   │   ├── HttpClientService.cs      # Typed HTTP client with OAuth token caching
│   │   ├── ValidationService.cs      # Request body validation
│   │   └── ProblemDetailsHelper.cs   # RFC 7807 error responses
│   ├── Middleware/
│   │   └── RequestHeaderMiddleware.cs # Correlation-ID propagation
│   ├── Model/                        # DTOs and constants
│   ├── ServiceImplementation/
│   │   └── SysService.cs             # Core orchestration service
│   ├── TransformAdapter/
│   │   └── TransformAdapter.cs       # External API key / base-URL resolver
│   └── TransformFactory/
│       └── TransformAdapterFactory.cs
└── GSI.IHUB.System.Service.Tests/    # xUnit unit-test project (Moq)
    ├── Configuration/
    ├── Functions/
    ├── Helpers/
    ├── ServiceImplementation/
    ├── Setup/
    ├── TransformAdapter/
    └── TransformFactory/
```

## Endpoint

| Method | Route | Auth |
|--------|-------|------|
| `POST` | `/api/system/process` | Function key (Bearer JWT) |

**Request body**

```json
{
  "requestId": "<guid>",
  "payload": "<json-string>"
}
```

Both `requestId` and `payload` are required. Missing or empty values return `400 Bad Request` with a `ProblemDetails` body.

## Run locally

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Azure Functions Core Tools v4](https://learn.microsoft.com/azure/azure-functions/functions-run-local)

### Steps

1. Copy and populate the environment config file for your target environment:

   ```bash
   # Example for dev
   cp GSI.IHUB.System.Service/Configuration/dev.json GSI.IHUB.System.Service/Configuration/dev.local.json
   ```

2. Edit the config file and supply real values for:
   - `AppSettings:ExternalApiBaseUrl`
   - `AppSettings:ExternalApiClientId` / `ClientSecret` / `TenantId` / `AuthorityUrl` (for OAuth)
   - `AppSettings:ExternalApiSubscriptionKey` (for APIM key auth)
   - `AppSettings:KeyVaultUri` (if using Key Vault)

3. Set the environment variable:

   ```bash
   export AZURE_ENVIRONMENT=dev   # dev | qa | uat | prd | dr | dvhf | perf | uthf
   ```

4. Restore, build, and run:

   ```bash
   dotnet restore GSI.IHUB.System.Service.sln
   dotnet build   GSI.IHUB.System.Service.sln
   func start --prefix GSI.IHUB.System.Service
   ```

5. Open the Swagger UI at `http://localhost:7071/api/swagger/ui`.

## Run tests

```bash
dotnet test GSI.IHUB.System.Service.sln
```

## Configuration reference (`AppSettings`)

| Key | Description |
|-----|-------------|
| `KeyVaultUri` | Azure Key Vault URI. When set, secrets are loaded via `DefaultAzureCredential`. |
| `ManagedIdentityClientId` | Client ID of a user-assigned managed identity for Key Vault access. |
| `ExternalApiBaseUrl` | Base URL of the downstream external API. |
| `ExternalApiSubscriptionKey` | APIM `Ocp-Apim-Subscription-Key` header value. |
| `ExternalApiVersion` | Appended as `?api-version=<value>` to the endpoint URL. |
| `ExternalApiClientId` | OAuth 2.0 client ID for client-credentials flow. |
| `ExternalApiClientSecret` | OAuth 2.0 client secret. |
| `ExternalApiTenantId` | Azure AD tenant ID. |
| `ExternalApiAuthorityUrl` | OAuth authority URL (e.g. `https://login.microsoftonline.com/<tenantId>`). |
| `CacheKeyExternalApi` | In-memory cache key for the OAuth access token (default: `ExternalApiToken`). |
| `HttpMaxRetry` | Maximum Polly retry attempts (default: `3`). |
| `HttpRetryDuration` | Base retry delay in seconds for exponential back-off (default: `2`). |

## Key Vault and Managed Identity

When `AppSettings:KeyVaultUri` is present the host adds an Azure Key Vault configuration provider using `DefaultAzureCredential`. To use a user-assigned managed identity set `AppSettings:ManagedIdentityClientId`.

## Resilience pipeline (Polly)

All outbound HTTP calls go through a named `system-service-pipeline` with three layers:

1. **Retry** — exponential back-off with jitter, configurable via `HttpMaxRetry` / `HttpRetryDuration`.
2. **Circuit breaker** — opens after ≥ 50 % failures over a 30-second sampling window (minimum 5 requests); stays open for 30 seconds.
3. **Per-attempt timeout** — 30 seconds.

## Observability

- **Correlation ID** — `RequestHeaderMiddleware` reads or generates an `x-correlation-id` header on every request and propagates it through all log entries and outbound calls.
- **Application Insights** — registered via `AddApplicationInsightsTelemetryWorkerService`.

## OpenAPI / Swagger

The `OpenApiConfigurationOptions` class is defined directly in `Program.cs` (namespace `GSI.IHUB.System.Service`) and registered in the DI container as `IOpenApiConfigurationOptions`. It configures:

- **Title**: GSI IHUB System API
- **Version**: v1.0.0
- **OpenAPI spec version**: 3.0 (V3)

Swagger UI is available at `/api/swagger/ui` and the raw spec at `/api/swagger.json`.

## Environment configuration files

| File | Environment |
|------|-------------|
| `Configuration/dev.json` | Development |
| `Configuration/qa.json` | QA |
| `Configuration/uat.json` | UAT |
| `Configuration/prd.json` | Production |
| `Configuration/dr.json` | Disaster Recovery |
| `Configuration/dvhf.json` | Dev High-Frequency |
| `Configuration/perf.json` | Performance |
| `Configuration/uthf.json` | UAT High-Frequency |
