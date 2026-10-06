namespace PackingSlip.Pdf.Domain.Models;

public sealed record Address(
    string Name,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PostalCode,
    string Country,
    string? Email,
    string? Phone,
    string? Vat);

public sealed record PackingSlipItem(
    string Sku,
    string ProductName,
    int Quantity);

public sealed record PackingSlip(
    string OrderNumber,
    DateTime OrderDate,
    string ShippingMethod,
    Address From,
    Address BillTo,
    Address ShipTo,
    IReadOnlyCollection<PackingSlipItem> Items,
    string? Notes,
    string? LogoBase64);
