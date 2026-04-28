using SystemApi.Contracts;

namespace SystemApi.TransformAdapter;

public class DCTransformAdapter : ITransformAdapter
{
    private readonly AppSettings _appSettings;

    public DCTransformAdapter(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public (string ApiKey, string SubscriptionKey) GetExternalAPIKeys()
    {
        return (
            _appSettings.ExternalApiSubscriptionKey,
            _appSettings.ExternalApiBaseUrl
        );
    }

    public async Task<string> ProcessRequestAsync(string requestData, string correlationId)
    {
        return await Task.FromResult("Request processed successfully");
    }
}
