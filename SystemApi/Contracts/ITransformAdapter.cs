namespace SystemApi.Contracts;

public interface ITransformAdapter
{
    (string ApiKey, string SubscriptionKey) GetExternalAPIKeys();
    Task<string> ProcessRequestAsync(string requestData, string correlationId);
}
