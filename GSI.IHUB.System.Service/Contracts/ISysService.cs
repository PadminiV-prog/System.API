using GSI.IHUB.System.Service.Model;

namespace GSI.IHUB.System.Service.Contracts;

public interface ISysService
{
    Task<string> ProcessRequestAsync(SystemRequest request, string correlationId);
}
