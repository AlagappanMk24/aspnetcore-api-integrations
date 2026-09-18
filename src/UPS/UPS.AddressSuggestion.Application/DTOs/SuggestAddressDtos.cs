namespace UPS.AddressSuggestion.Application.DTOs;

public sealed record SuggestAddressCommand(
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
    int MaximumCandidateListSize = 5)
    : MediatR.IRequest<AddressSuggestionResultDto>;

public sealed record AddressSuggestionResultDto(
    bool IsValidAddress,
    bool IsAmbiguous,
    bool HasSuggestions,
    IReadOnlyCollection<AddressSuggestionCandidateDto> Suggestions,
    AddressSuggestionStatusDto Status,
    IReadOnlyCollection<AddressSuggestionAlertDto> Alerts);

public sealed record AddressSuggestionCandidateDto(
    string? ConsigneeName,
    IReadOnlyCollection<string> AddressLines,
    string? City,
    string? State,
    string? PostalCode,
    string? PostalCodeExtension,
    string? Urbanization,
    string? Region,
    string CountryCode,
    string? ClassificationCode,
    string? ClassificationDescription);

public sealed record AddressSuggestionStatusDto(
    string Code,
    string Description,
    string? TransactionContext);

public sealed record AddressSuggestionAlertDto(
    string Code,
    string Description);

public sealed record ApiErrorDto(
    string Code,
    string Message);
