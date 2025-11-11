using CampusEats.Api.Domain.Enums;
using CampusEats.Api.Features.KitchenTask;
using FluentValidation;

namespace CampusEats.Api.Validators;

// Un validator pentru comanda de update status
public class UpdateTaskStatusValidator : AbstractValidator<UpdateTaskStatusCommand>
{
    public UpdateTaskStatusValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        
        RuleFor(x => x.NewStatus).NotEmpty()
            .Must(statusStr => Enum.TryParse<KitchenTaskStatus>(statusStr, true, out _))
            .WithMessage("Invalid or unrecognized status value.");
    }
}

// Un validator pentru comanda de asignare
public class AssignTaskToStaffValidator : AbstractValidator<AssignTaskToStaffCommand>
{
    public AssignTaskToStaffValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.StaffId).NotEmpty();
    }
}