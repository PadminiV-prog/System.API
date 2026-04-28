using SystemApi.Model;

namespace SystemApi.Contracts;

public interface IHttpClientService
{
    Task<string> PostAsync(ExternalApiRequestModel model, string correlationId);
}
