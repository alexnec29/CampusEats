using CampusEats.Api.Models;
using FluentValidation;

namespace CampusEats.Api.Validators;

public class WorkingHoursValidator : AbstractValidator<WorkingHours>
{
    public WorkingHoursValidator()
    {
        RuleFor(x => x.Open)
            .NotEmpty().WithMessage("Open is required")
            .Must(t => t >= TimeSpan.Zero && t < TimeSpan.FromDays(1))
            .WithMessage("Open must be between 00:00 and 23:59:59");
        
        RuleFor(x => x.Close)
            .NotEmpty().WithMessage("Close is required")
            .Must(t => t >= TimeSpan.Zero && t < TimeSpan.FromDays(1))
            .WithMessage("Close must be between 00:00 and 23:59:59");
    }
}