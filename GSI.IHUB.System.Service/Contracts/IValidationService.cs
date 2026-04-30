using GSI.IHUB.System.Service.Model;

namespace GSI.IHUB.System.Service.Contracts;

public interface IValidationService
{
    (bool IsValid, SystemRequest? Request) ValidateRequest(string? body, string correlationId);
}
