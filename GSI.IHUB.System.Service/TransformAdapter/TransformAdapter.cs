using GSI.IHUB.System.Service.Contracts;

namespace GSI.IHUB.System.Service.TransformAdapter;

public class TransformAdapter : ITransformAdapter
{
    private readonly AppSettings _appSettings;

    public TransformAdapter(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public (string ApiKey, string BaseUrl) GetExternalAPIKeys()
    {
        return (
            _appSettings.ExternalApiSubscriptionKey,
            _appSettings.ExternalApiBaseUrl
        );
    }

    public async Task<string> ProcessRequestAsync(string requestData, string apiKey, string baseUrl, string correlationId)
    {
        return await Task.FromResult("Request processed successfully");
    }
}
