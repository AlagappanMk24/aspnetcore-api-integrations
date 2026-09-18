using MediatR;
using UPS.AddressSuggestion.Application.Contracts;
using UPS.AddressSuggestion.Application.DTOs;

namespace UPS.AddressSuggestion.Application.Handlers;

public sealed class SuggestAddressCommandHandler(
    IAddressSuggestionService addressSuggestionService)
    : IRequestHandler<SuggestAddressCommand, AddressSuggestionResultDto>
{
    public Task<AddressSuggestionResultDto> Handle(
        SuggestAddressCommand request,
        CancellationToken cancellationToken)
        => addressSuggestionService.SuggestAsync(request, cancellationToken);
}
