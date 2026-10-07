namespace PackingSlip.Pdf.Domain.Models;

/// <summary>
/// Data bound to the supplied Commercial Packing Slip PDF template.
/// The template is a flattened PDF, so values are overlaid at fixed template coordinates.
/// </summary>
public sealed record TemplatePackingSlip(
    string InvoiceNumber,
    string OrderNumber,
    DateTime OrderDate,
    string DeliveryMethod,
    TemplateCompanyContact Company,
    TemplateAddress BillTo,
    TemplateAddress ShipTo,
    IReadOnlyCollection<TemplatePackingSlipItem> Items,
    string? FooterContactMessage = null,
    string? FooterThankYouMessage = null);

public sealed record TemplateCompanyContact(
    string Phone,
    string Email,
    string Website);

public sealed record TemplateAddress(
    string Name,
    string AddressLine1,
    string CityStateZip,
    string? EmailAndPhone);

public sealed record TemplatePackingSlipItem(
    string ItemNumber,
    string Description,
    int Quantity);
