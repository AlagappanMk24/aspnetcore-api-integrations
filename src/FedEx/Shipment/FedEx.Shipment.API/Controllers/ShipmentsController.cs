using FedEx.Shipment.Application.Shipments;
using FedEx.Shipment.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FedEx.Shipment.API.Controllers;

[ApiController]
[Route("api/shipments")]
public sealed class ShipmentsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateShipmentResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateShipmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateShipmentCommand(
            request.Shipper,
            request.Recipient,
            request.ServiceType,
            request.PackagingType,
            request.PickupType,
            request.LabelFormat,
            request.Weight,
            request.Dimensions);

        var result = await mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<object>(
            Success: true,
            Message: "Shipment created successfully",
            StatusCode: StatusCodes.Status200OK,
            Data: result,
            TraceId: HttpContext.TraceIdentifier));
    }
}

public sealed record CreateShipmentRequest(
    Address Shipper,
    Address Recipient,
    string ServiceType,
    string PackagingType,
    string PickupType,
    string LabelFormat,
    PackageWeight Weight,
    PackageDimensions Dimensions);

public sealed record ApiResponse<T>(
    bool Success,
    string Message,
    int StatusCode,
    T Data,
    string TraceId);