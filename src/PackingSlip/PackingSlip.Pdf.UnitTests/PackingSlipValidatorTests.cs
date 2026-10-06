using PackingSlip.Pdf.Application.Services;
using PackingSlip.Pdf.Domain.Models;
using Xunit;

namespace PackingSlip.Pdf.UnitTests;

public sealed class PackingSlipValidatorTests
{
    [Fact]
    public void Validate_WhenValid_ReturnsNoErrors()
    {
        Assert.Empty(PackingSlipValidator.Validate(CreateRequest()));
    }

    [Fact]
    public void Validate_WhenNoItems_ReturnsError()
    {
        var request = CreateRequest() with { Items = [] };
        Assert.Contains(PackingSlipValidator.Validate(request), x => x.Contains("At least one item"));
    }

    private static Domain.Models.PackingSlip CreateRequest() => new(
        "123456",
        new DateTime(2025, 4, 15),
        "DHL",
        new Address("Litmovement", "Suite 12, Karma Plaza", "Store 7, Shopping District, Cupertino", "California", "CA", "95014", "United States (US)", null, null, null),
        new Address("Billing address name", "20 Maple Avenue", null, "San Pedro", "California", "90731", "United States (US)", "info@example.com", "+1 123 456", "123456"),
        new Address("Shipping address name", "20 Maple Avenue", null, "San Pedro", "California", "90731", "United States (US)", null, null, null),
        [new PackingSlipItem("A1234", "Jumbing LED Light Wall Ball", 5)],
        null,
        null);
}
