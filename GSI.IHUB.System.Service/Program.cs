using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using GSI.IHUB.System.Service;
using GSI.IHUB.System.Service.Configuration;
using GSI.IHUB.System.Service.Contracts;
using GSI.IHUB.System.Service.Helpers;
using GSI.IHUB.System.Service.Middleware;
using GSI.IHUB.System.Service.Model;
using GSI.IHUB.System.Service.ServiceImplementation;
using GSI.IHUB.System.Service.TransformFactory;

var environment = Environment.GetEnvironmentVariable("AZURE_ENVIRONMENT")?.ToLowerInvariant() ?? "dev";

var host = new HostBuilder()
    .ConfigureAppConfiguration((_, configBuilder) =>
    {
        configBuilder
            .AddJsonFile("host.json", optional: true, reloadOnChange: true)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"Configuration/{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        var interimConfiguration = configBuilder.Build();
        var keyVaultUri = interimConfiguration["AppSettings:KeyVaultUri"] ?? interimConfiguration["KeyVaultUri"];
        var managedIdentityClientId = interimConfiguration["AppSettings:ManagedIdentityClientId"] ?? interimConfiguration["ManagedIdentityClientId"];

        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            configBuilder.AddAzureKeyVault(
                new Uri(keyVaultUri),
                string.IsNullOrWhiteSpace(managedIdentityClientId)
                    ? new DefaultAzureCredential()
                    : new DefaultAzureCredential(new DefaultAzureCredentialOptions
                    {
                        ManagedIdentityClientId = managedIdentityClientId
                    }));
        }
    })
    .ConfigureFunctionsWebApplication(workerApplication =>
    {
        workerApplication.UseMiddleware<RequestHeaderMiddleware>();
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<AppSettings>(context.Configuration.GetSection("AppSettings"));
        services.AddSingleton(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AppSettings>>().Value);
        services.AddMemoryCache();

        // Resolve Polly settings from config, falling back to built-in defaults
        var maxRetry = int.TryParse(
            context.Configuration["AppSettings:HttpMaxRetry"],
            out var r) ? r : ApplicationConstants.MaxRetryCount;

        var retryDelay = int.TryParse(
            context.Configuration["AppSettings:HttpRetryDuration"],
            out var d) ? d : ApplicationConstants.PollyRetryInterval;

        services.AddHttpClient(ApplicationConstants.HttpClientName)
            .AddResilienceHandler("system-service-pipeline", builder =>
            {
                // Retry with exponential back-off
                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = maxRetry,
                    Delay = TimeSpan.FromSeconds(retryDelay),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true
                });

                // Circuit-breaker: open after 50% failures over a 30-second window
                builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 5,
                    BreakDuration = TimeSpan.FromSeconds(30)
                });

                // Per-attempt timeout
                builder.AddTimeout(TimeSpan.FromSeconds(30));
            });

        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IHttpClientService, HttpClientService>();
        services.AddScoped<ISysService, SysService>();
        services.AddScoped<ITransformAdapterFactory, TransformAdapterFactory>();

        services.AddSingleton<IOpenApiConfigurationOptions, OpenApiConfigurationOptions>();

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

await host.RunAsync();
