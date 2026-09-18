using MediatR;

namespace UPS.AddressValidation.Application.DTOs;

public sealed record ValidateAddressCommand(
    string? ConsigneeName,
    IReadOnlyCollection<string> AddressLines,
    string? City,
    string? State,
    string? PostalCode,
    string? PostalCodeExtension,
    string? Urbanization,
    string CountryCode,
    string? Region,
    bool RegionalRequestIndicator = false,
    int RequestOption = 1,
    int MaximumCandidateListSize = 15
) : IRequest<AddressValidationResultDto>;
