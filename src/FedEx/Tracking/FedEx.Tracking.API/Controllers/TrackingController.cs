using FedEx.Tracking.Application.Tracking;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace FedEx.Tracking.API.Controllers;

[ApiController]
[Route("api/tracking")]
public sealed class TrackingController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Track([FromBody] TrackRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new TrackShipmentsQuery(request.TrackingNumbers, request.IncludeDetailedScans), ct);

        return Ok(new
        {
            Success = true,
            Message = "Tracking request completed.",
            StatusCode = 200,
            Data = result,
            TraceId = HttpContext.TraceIdentifier
        });
    }
}
public sealed record TrackRequest(
    IReadOnlyCollection<string> TrackingNumbers, 
    bool IncludeDetailedScans = true);