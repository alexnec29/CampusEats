using CampusEats.Api.Models;
using FluentValidation;

namespace CampusEats.Api.Validators;

public class WeeklyWorkingHoursValidator : AbstractValidator<WeeklyWorkingHours>
{
    public WeeklyWorkingHoursValidator(WorkingHoursValidator workingHoursValidator)
    {
        RuleFor(x => x.Monday)
            .NotEmpty().WithMessage("Monday is required")
            .SetValidator(workingHoursValidator);
        
        RuleFor(x => x.Tuesday)
            .NotEmpty().WithMessage("Tuesday is required")
            .SetValidator(workingHoursValidator);
        
        RuleFor(x => x.Wednesday)
            .NotEmpty().WithMessage("Wednesday is required")
            .SetValidator(workingHoursValidator);
        
        RuleFor(x => x.Thursday)
            .NotEmpty().WithMessage("Thursday is required")
            .SetValidator(workingHoursValidator);
        
        RuleFor(x => x.Friday)
            .NotEmpty().WithMessage("Friday is required")
            .SetValidator(workingHoursValidator);
        
        RuleFor(x => x.Saturday)
            .NotEmpty().WithMessage("Saturday is required")
            .SetValidator(workingHoursValidator);
        
        RuleFor(x => x.Sunday)
            .NotEmpty().WithMessage("Sunday is required")
            .SetValidator(workingHoursValidator);
    }
}