using GSI.IHUB.System.Service.Model;

namespace GSI.IHUB.System.Service.Contracts;

public interface IValidationService
{
    Task<bool> ValidateRequestAsync(SystemRequest request);
}
