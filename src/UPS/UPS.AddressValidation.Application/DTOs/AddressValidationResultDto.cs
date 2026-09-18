namespace UPS.AddressValidation.Application.DTOs;

public sealed record AddressValidationResultDto(
    bool IsValid,
    bool IsAmbiguous,
    bool HasCandidates,
    string? StatusCode,
    string? StatusDescription,
    AddressClassificationDto? Classification,
    IReadOnlyCollection<AddressCandidateDto> Candidates,
    IReadOnlyCollection<AddressAlertDto> Alerts);

public sealed record AddressCandidateDto(
    string? ConsigneeName,
    IReadOnlyCollection<string> AddressLines,
    string? City,
    string? State,
    string? PostalCode,
    string? PostalCodeExtension,
    string? Urbanization,
    string? CountryCode,
    string? Region,
    AddressClassificationDto? Classification);

public sealed record AddressClassificationDto(
    string? Code,
    string? Description);

public sealed record AddressAlertDto(
    string? Code,
    string? Description);

public sealed record ApiErrorDto(
    string Code,
    string Message);
