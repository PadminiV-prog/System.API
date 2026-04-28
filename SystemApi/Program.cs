using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SystemApi;
using SystemApi.Contracts;
using SystemApi.Helpers;
using SystemApi.Middleware;
using SystemApi.Model;
using SystemApi.ServiceImplementation;
using SystemApi.TransformFactory;

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
        services.AddHttpClient(ApplicationConstants.HttpClientName);

        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IHttpClientService, HttpClientService>();
        services.AddScoped<ISysService, SysService>();
        services.AddScoped<ITransformAdapterFactory, TransformAdapterFactory>();

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

await host.RunAsync();
