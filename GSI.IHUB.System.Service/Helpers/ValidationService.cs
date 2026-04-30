using Microsoft.Extensions.Logging;
using GSI.IHUB.System.Service.Contracts;
using GSI.IHUB.System.Service.Model;

namespace GSI.IHUB.System.Service.Helpers;

public class ValidationService : IValidationService
{
    private readonly ILogger<ValidationService> _logger;

    public ValidationService(ILogger<ValidationService> logger)
    {
        _logger = logger;
    }

    public Task<bool> ValidateRequestAsync(SystemRequest request)
    {
        var isValid = request is not null &&
                      !string.IsNullOrWhiteSpace(request.RequestId) &&
                      !string.IsNullOrWhiteSpace(request.Payload);

        _logger.LogInformation("System request validation result: {ValidationResult}", isValid);
        return Task.FromResult(isValid);
    }
}
