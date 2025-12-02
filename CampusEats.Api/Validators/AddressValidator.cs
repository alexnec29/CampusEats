using CampusEats.Api.Models;
using FluentValidation;

namespace CampusEats.Api.Validators;

public class AddressValidator : AbstractValidator<Address>
{
    public AddressValidator()
    {
        RuleFor(a => a.street)
            .NotEmpty().WithMessage("Street is required")
            .MaximumLength(200).WithMessage("Street is too long");

        RuleFor(a => a.building)
            .NotEmpty().WithMessage("Building is required")
            .MaximumLength(100).WithMessage("Building is too long");

        RuleFor(a => a.city)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City is too long");

        RuleFor(a => a.county)
            .NotEmpty().WithMessage("County is required")
            .MaximumLength(100).WithMessage("County is too long");
    }
}