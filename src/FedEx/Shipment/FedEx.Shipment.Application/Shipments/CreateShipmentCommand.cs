using FedEx.Shipment.Domain.Models;
using FluentValidation;
using MediatR;
using System.Net;

namespace FedEx.Shipment.Application.Shipments;

public sealed record CreateShipmentCommand(
    Address Shipper,
    Address Recipient,
    string ServiceType,
    string PackagingType,
    string PickupType,
    string LabelFormat,
    PackageWeight Weight,
    PackageDimensions Dimensions) : IRequest<CreateShipmentResult>;

public sealed class CreateShipmentCommandValidator : AbstractValidator<CreateShipmentCommand>
{
    public CreateShipmentCommandValidator()
    {
        RuleFor(x => x.Shipper)
            .NotNull();

        RuleFor(x => x.Recipient)
            .NotNull();

        RuleFor(x => x.ServiceType)
            .NotEmpty();

        RuleFor(x => x.PackagingType)
            .NotEmpty();

        RuleFor(x => x.PickupType)
            .NotEmpty();

        RuleFor(x => x.LabelFormat)
            .Must(x => x is "PDF" or "PNG")
            .WithMessage("Label format must be 'PDF' or 'PNG'.");

        RuleFor(x => x.Weight.Value)
            .GreaterThan(0);

        RuleFor(x => x.Weight.Units)
            .Must(x => x is "LB" or "KG")
            .WithMessage("Weight units must be 'LB' or 'KG'.");

        RuleFor(x => x.Dimensions.Length)
            .GreaterThan(0);

        RuleFor(x => x.Dimensions.Width)
            .GreaterThan(0);

        RuleFor(x => x.Dimensions.Height)
            .GreaterThan(0);
    }
}