namespace Vertex.Tax.Domain.Models;

public sealed record TaxAddress(
    string Country,
    string? State = null,
    string? City = null,
    string? PostalCode = null,
    string? StreetAddress1 = null
);

public sealed record TaxLine(
    int LineNumber,
    decimal Quantity,
    decimal UnitPrice,
    string? ProductClass = null,
    string? ProductCode = null
);

public sealed record TaxCalculation(
    string SaleMessageType,
    string DocumentNumber,
    DateOnly DocumentDate,
    string Currency,
    TaxAddress Seller,
    TaxAddress Customer,
    IReadOnlyList<TaxLine> Lines
);

public sealed record TaxLineResult(
    int LineNumber,
    decimal? TaxAmount,
    decimal? ExtendedPrice,
    string? Jurisdiction
);

public sealed record TaxCalculationResult(
    decimal? TotalTax,
    decimal? TotalTaxable,
    IReadOnlyList<TaxLineResult> Lines,
    string RawResponse
);