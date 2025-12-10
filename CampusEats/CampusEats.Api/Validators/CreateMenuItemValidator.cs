using CampusEats.Api.Features.MenuItem;
using FluentValidation;

namespace CampusEats.Api.Validators;

public class CreateMenuItemValidator : AbstractValidator<CreateMenuItemRequest>
{
    public CreateMenuItemValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Category).IsInEnum();
    }
}
