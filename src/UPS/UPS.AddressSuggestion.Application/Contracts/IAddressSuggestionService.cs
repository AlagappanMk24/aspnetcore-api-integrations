using UPS.AddressSuggestion.Application.DTOs;

namespace UPS.AddressSuggestion.Application.Contracts;

public interface IAddressSuggestionService
{
    Task<AddressSuggestionResultDto> SuggestAsync(
        SuggestAddressCommand command,
        CancellationToken cancellationToken = default);
}
