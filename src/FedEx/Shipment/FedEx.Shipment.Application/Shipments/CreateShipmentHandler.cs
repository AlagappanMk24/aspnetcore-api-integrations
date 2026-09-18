using FedEx.Shipment.Application.Abstractions;
using FedEx.Shipment.Domain.Models;
using MediatR;

namespace FedEx.Shipment.Application.Shipments;

public sealed class CreateShipmentHandler(IFedExShipmentService shipmentService)
    : IRequestHandler<CreateShipmentCommand, CreateShipmentResult>
{
    public Task<CreateShipmentResult> Handle(
        CreateShipmentCommand request,
        CancellationToken cancellationToken) =>
        shipmentService.CreateAsync(request, cancellationToken);
}