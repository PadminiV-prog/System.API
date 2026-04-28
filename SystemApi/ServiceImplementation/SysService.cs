using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SystemApi.Contracts;
using SystemApi.Model;

namespace SystemApi.ServiceImplementation;

public class SysService : ISysService
{
    private readonly ILogger<SysService> _logger;
    private readonly AppSettings _appSettings;
    private readonly IHttpClientService _httpClientService;
    private readonly ITransformAdapterFactory _transformAdapterFactory;

    public SysService(
        ILogger<SysService> logger,
        AppSettings appSettings,
        IHttpClientService httpClientService,
        ITransformAdapterFactory transformAdapterFactory)
    {
        _logger = logger;
        _appSettings = appSettings;
        _httpClientService = httpClientService;
        _transformAdapterFactory = transformAdapterFactory;
    }

    public async Task<string> ProcessRequestAsync(SystemRequest request, string correlationId)
    {
        try
        {
            _logger.LogInformation($"SysService ProcessRequestAsync-> Method entered for correlationId :{correlationId}");

            if (!GetTransformAdapter(out var adapter, out var errorMessage, correlationId))
            {
                return errorMessage ?? "No adapter available.";
            }

            var (apiKey, baseUrl) = adapter!.GetExternalAPIKeys();
            var requestData = JsonConvert.SerializeObject(request);

            var model = new ExternalApiRequestModel
            {
                BaseUrl = baseUrl,
                ClientId = _appSettings.ExternalApiClientId,
                ClientSecret = _appSettings.ExternalApiClientSecret,
                AuthorityUrl = _appSettings.ExternalApiAuthorityUrl,
                SubscriptionKey = apiKey,
                ApiVersion = _appSettings.ExternalApiVersion,
                CacheKey = string.IsNullOrWhiteSpace(_appSettings.CacheKeyExternalApi)
                    ? ApplicationConstants.CacheKeyExternalApi
                    : _appSettings.CacheKeyExternalApi,
                Scope = BuildDefaultScope(baseUrl),
                RequestData = requestData,
                CorrelationId = correlationId
            };

            var result = await _httpClientService.PostAsync(model, correlationId);
            return string.IsNullOrWhiteSpace(result) ? "NoResponse" : result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"SysService ProcessRequestAsync-> exception occurred and exception message :{ex.Message} for correlationId :{correlationId}");
            throw;
        }
    }

    private bool GetTransformAdapter(out ITransformAdapter? adapter, out string? errorMessage, string correlationId)
    {
        adapter = _transformAdapterFactory.GetAdapter();
        errorMessage = null;

        if (adapter is null)
        {
            _logger.LogError($"SysService ProcessRequestAsync-> No adapter found for correlationId :{correlationId}");
            errorMessage = "No adapter found.";
            return false;
        }

        return true;
    }

    private static string BuildDefaultScope(string baseUrl)
    {
        return string.IsNullOrWhiteSpace(baseUrl)
            ? "https://management.azure.com/.default"
            : $"{baseUrl.TrimEnd('/')}/.default";
    }
}

