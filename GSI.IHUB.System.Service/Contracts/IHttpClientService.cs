using GSI.IHUB.System.Service.Model;

namespace GSI.IHUB.System.Service.Contracts;

public interface IHttpClientService
{
    Task<string> PostAsync(ExternalApiRequestModel model, string correlationId);
}
