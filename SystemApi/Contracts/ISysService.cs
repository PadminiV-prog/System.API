using SystemApi.Model;

namespace SystemApi.Contracts;

public interface ISysService
{
    Task<string> ProcessRequestAsync(SystemRequest request, string correlationId);
}
