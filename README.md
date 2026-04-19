# System.API

System API is a pass-through Azure Function API that validates incoming requests and relays them to an external API without transforming payloads.

## Solution structure

- `SystemApi` - .NET 10 Azure Functions isolated worker project
- `SystemApi.Tests` - xUnit unit tests

## Run locally

1. Update `SystemApi/Configuration/dev.json` (or `qa.json`) with local values.
2. Ensure `AZURE_ENVIRONMENT` is set (`dev` or `qa`).
3. Run:

```bash
dotnet restore /home/runner/work/System.API/System.API/SystemApi.sln
dotnet build /home/runner/work/System.API/System.API/SystemApi.sln
dotnet test /home/runner/work/System.API/System.API/SystemApi.sln
```

## Key Vault and Managed Identity

- Configuration reads `AppSettings:KeyVaultUri` and loads secrets using `DefaultAzureCredential`.
- Set `AppSettings:ManagedIdentityClientId` when user-assigned managed identity is required.

## Environment configuration

- `SystemApi/Configuration/dev.json`
- `SystemApi/Configuration/qa.json`
