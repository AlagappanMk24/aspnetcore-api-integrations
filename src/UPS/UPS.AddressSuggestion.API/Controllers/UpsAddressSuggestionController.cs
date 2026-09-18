using MediatR;
using Microsoft.AspNetCore.Mvc;
using UPS.AddressSuggestion.Application.DTOs;

namespace UPS.AddressSuggestion.API.Controllers;

[ApiController]
[Route("api/ups/address-suggestion")]
public sealed class UpsAddressSuggestionController(IMediator mediator)
    : ControllerBase
{
    /// <summary>
    /// Gets UPS candidate address suggestions for an entered address.
    /// </summary>
    [HttpPost("suggest")]
    [ProducesResponseType(typeof(AddressSuggestionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<AddressSuggestionResultDto>> Suggest(
        [FromBody] SuggestAddressCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
