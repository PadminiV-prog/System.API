namespace GSI.IHUB.System.Service.Contracts;

public interface ITransformAdapter
{
    (string ApiKey, string BaseUrl) GetExternalAPIKeys();
    Task<string> ProcessRequestAsync(string requestData, string apiKey, string baseUrl, string correlationId);
}
