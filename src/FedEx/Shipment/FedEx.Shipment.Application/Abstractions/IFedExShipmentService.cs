using FedEx.Shipment.Domain.Models;
using FedEx.Shipment.Application.Shipments;
namespace FedEx.Shipment.Application.Abstractions;
public interface IFedExShipmentService
{
    Task<CreateShipmentResult> CreateAsync(CreateShipmentCommand command, CancellationToken ct);
}