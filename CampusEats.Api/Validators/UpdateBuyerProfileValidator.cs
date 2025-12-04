using CampusEats.Api.Features.User;
using FluentValidation;

namespace CampusEats.Api.Validators;

public class UpdateBuyerProfileValidator : AbstractValidator<UpdateBuyerProfileRequest>
{
    public UpdateBuyerProfileValidator(AddressValidator addressValidator)
    {
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required");
        
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required");
        
        RuleFor(x => x.Age)
            .NotEmpty().WithMessage("Age is required")
            .GreaterThan(0).WithMessage("Age must be greater than 0")
            .LessThanOrEqualTo(150).WithMessage("Age must be less than 150");

        RuleFor(x => x.DeliveryAddress)
            .NotEmpty().WithMessage("DeliveryAddress is required")
            .SetValidator(addressValidator);
    }
}