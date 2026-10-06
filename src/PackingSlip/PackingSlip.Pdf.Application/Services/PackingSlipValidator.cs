using PackingSlip.Pdf.Domain.Models;

namespace PackingSlip.Pdf.Application.Services;

public static class PackingSlipValidator
{
    public static IReadOnlyList<string> Validate(Domain.Models.PackingSlip request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.OrderNumber)) errors.Add("OrderNumber is required.");
        if (string.IsNullOrWhiteSpace(request.ShippingMethod)) errors.Add("ShippingMethod is required.");
        if (request.Items is null || request.Items.Count == 0) errors.Add("At least one item is required.");
        if (request.Items?.Any(i => i.Quantity <= 0) == true) errors.Add("Item quantity must be greater than zero.");

        ValidateAddress("From", request.From, errors);
        ValidateAddress("BillTo", request.BillTo, errors);
        ValidateAddress("ShipTo", request.ShipTo, errors);

        return errors;
    }

    private static void ValidateAddress(string prefix, Address address, ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(address.Name)) errors.Add($"{prefix}.Name is required.");
        if (string.IsNullOrWhiteSpace(address.AddressLine1)) errors.Add($"{prefix}.AddressLine1 is required.");
        if (string.IsNullOrWhiteSpace(address.City)) errors.Add($"{prefix}.City is required.");
        if (string.IsNullOrWhiteSpace(address.State)) errors.Add($"{prefix}.State is required.");
        if (string.IsNullOrWhiteSpace(address.PostalCode)) errors.Add($"{prefix}.PostalCode is required.");
        if (string.IsNullOrWhiteSpace(address.Country)) errors.Add($"{prefix}.Country is required.");
    }
}
