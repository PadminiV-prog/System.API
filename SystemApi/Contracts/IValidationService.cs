using SystemApi.Model;

namespace SystemApi.Contracts;

public interface IValidationService
{
    Task<bool> ValidateRequestAsync(SystemRequest request);
}
