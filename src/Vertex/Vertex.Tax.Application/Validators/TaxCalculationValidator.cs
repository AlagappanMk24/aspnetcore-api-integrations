using FluentValidation;
using Vertex.Tax.Domain.Models;

namespace Vertex.Tax.Application.Validators;
public sealed class TaxCalculationValidator : AbstractValidator<TaxCalculation>
{
    public TaxCalculationValidator()
    {
        RuleFor(x => x.SaleMessageType).Must(x => new[] { "Quotation", "Invoice", "DistributeTax" }.Contains(x, StringComparer.OrdinalIgnoreCase));
        RuleFor(x => x.DocumentNumber).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Seller.Country).NotEmpty().Length(2);
        RuleFor(x => x.Customer.Country).NotEmpty().Length(2);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).ChildRules(l => { l.RuleFor(x => x.LineNumber).GreaterThan(0); l.RuleFor(x => x.Quantity).GreaterThan(0); l.RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0); });
    }
}