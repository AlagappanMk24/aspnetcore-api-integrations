using UPS.AddressValidation.Application.DTOs;

namespace UPS.AddressValidation.Application.Contracts;

public interface IAddressValidationService
{
    Task<AddressValidationResultDto> ValidateAsync(
        ValidateAddressCommand command,
        CancellationToken cancellationToken = default);
}
