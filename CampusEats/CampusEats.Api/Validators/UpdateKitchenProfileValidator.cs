using CampusEats.Api.Features.User;
using FluentValidation;

namespace CampusEats.Api.Validators;

public class UpdateKitchenProfileValidator : AbstractValidator<UpdateKitchenProfileRequest>
{
    public UpdateKitchenProfileValidator(AddressValidator addressValidator, WeeklyWorkingHoursValidator weeklyWorkingHoursValidator)
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("CompanyName should not be empty")
            .MaximumLength(50).WithMessage("CompanyName must not exceed 50 characters");
        
        RuleFor(x => x.KitchenAddress)
            .NotEmpty().WithMessage("KitchenAddress should not be empty")
            .SetValidator(addressValidator);

        RuleFor(x => x.WeeklyWorkingHours)
            .NotEmpty().WithMessage("WeeklyWorkingHours should not be empty")
            .SetValidator(weeklyWorkingHoursValidator);

    }
}