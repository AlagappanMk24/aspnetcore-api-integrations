using FedEx.Shipment.Application.Shipments;
using FedEx.Shipment.Domain.Models;
using System.Net;
using Xunit;
namespace FedEx.Shipment.Tests;
public sealed class ValidatorTests
{
    [Fact]
    public void ValidRequestPasses()
    {
        // Arrange
        var address = new Address(
            PersonName: "Test",
            PhoneNumber: "5555550100",
            CompanyName: null,
            StreetLines: ["1 Main"],
            City: "Memphis",
            StateOrProvinceCode: "TN",
            PostalCode: "38103",
            CountryCode: "US");

        var command = new CreateShipmentCommand(
                    Shipper: address,
                    Recipient: address,
                    ServiceType: "FEDEX_GROUND",
                    PackagingType: "YOUR_PACKAGING",
                    PickupType: "DROPOFF_AT_FEDEX_LOCATION",
                    LabelFormat: "PDF",
                    Weight: new PackageWeight("LB", 2),
                    Dimensions: new PackageDimensions(10, 8, 4, "IN"));

        var validator = new CreateShipmentCommandValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }
}