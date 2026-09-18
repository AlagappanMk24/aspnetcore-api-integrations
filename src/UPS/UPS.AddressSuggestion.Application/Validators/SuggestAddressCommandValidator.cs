using FluentValidation;
using UPS.AddressSuggestion.Application.DTOs;

namespace UPS.AddressSuggestion.Application.Validators;

public sealed class SuggestAddressCommandValidator : AbstractValidator<SuggestAddressCommand>
{
    public SuggestAddressCommandValidator()
    {
        RuleFor(x => x.AddressLines)
            .NotNull()
            .Must(x => x.Count is >= 1 and <= 3)
            .WithMessage("AddressLines must contain between 1 and 3 lines.");

        RuleForEach(x => x.AddressLines)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ConsigneeName)
            .MaximumLength(40)
            .When(x => x.ConsigneeName is not null);

        RuleFor(x => x.City)
            .MaximumLength(30)
            .When(x => x.City is not null);

        RuleFor(x => x.State)
            .MaximumLength(30)
            .When(x => x.State is not null);

        RuleFor(x => x.PostalCode)
            .MaximumLength(10)
            .When(x => x.PostalCode is not null);

        RuleFor(x => x.PostalCodeExtension)
            .MaximumLength(10)
            .When(x => x.PostalCodeExtension is not null);

        RuleFor(x => x.Urbanization)
            .MaximumLength(30)
            .When(x => x.Urbanization is not null);

        RuleFor(x => x.Region)
            .MaximumLength(100)
            .When(x => x.Region is not null);

        RuleFor(x => x.CountryCode)
            .NotEmpty()
            .Length(2)
            .Matches("^[A-Za-z]{2}$")
            .WithMessage("CountryCode must be a two-letter ISO-style country code.");

        RuleFor(x => x.MaximumCandidateListSize)
            .InclusiveBetween(1, 50);
    }
}
