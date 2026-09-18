namespace FedEx.Shipment.Domain.Models;
public sealed record Address(
    string PersonName,
    string PhoneNumber,
    string? CompanyName,
    string[] StreetLines,
    string City,
    string StateOrProvinceCode,
    string PostalCode,
    string CountryCode);
public sealed record PackageWeight(string Units,double Value);
public sealed record PackageDimensions(int Length,int Width,int Height,string Units);
public sealed record CreateShipmentResult(string? TransactionId,string? TrackingNumber,string? EncodedLabel,string? LabelContentType);