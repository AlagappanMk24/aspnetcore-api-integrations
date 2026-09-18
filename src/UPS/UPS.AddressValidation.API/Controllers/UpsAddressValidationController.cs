using MediatR;
using Microsoft.AspNetCore.Mvc;
using UPS.AddressValidation.Application.DTOs;

namespace UPS.AddressValidation.API.Controllers;

[ApiController]
[Route("api/ups/address-validation")]
public sealed class UpsAddressValidationController(
    ISender sender)
    : ControllerBase
{
    [HttpPost("validate")]
    [ProducesResponseType(
        typeof(AddressValidationResultDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<AddressValidationResultDto>> Validate(
        [FromBody] ValidateAddressCommand command,
        CancellationToken cancellationToken)
    {
        var result =
            await sender.Send(
                command,
                cancellationToken);

        return Ok(result);
    }
}
