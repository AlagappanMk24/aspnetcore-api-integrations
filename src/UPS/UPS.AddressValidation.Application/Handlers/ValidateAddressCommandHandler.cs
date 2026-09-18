using MediatR;
using UPS.AddressValidation.Application.Contracts;
using UPS.AddressValidation.Application.DTOs;

namespace UPS.AddressValidation.Application.Handlers;

public sealed class ValidateAddressCommandHandler(
    IAddressValidationService addressValidationService)
    : IRequestHandler<ValidateAddressCommand, AddressValidationResultDto>
{
    public Task<AddressValidationResultDto> Handle(
        ValidateAddressCommand request,
        CancellationToken cancellationToken)
    {
        return addressValidationService.ValidateAsync(request, cancellationToken);
    }
}
