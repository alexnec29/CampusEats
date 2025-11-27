using CampusEats.Api.Features.KitchenTask;
using CampusEats.Api.Models.Enums;
using FluentValidation;

namespace CampusEats.Api.Validators;

// Un validator pentru comanda de update status
public class UpdateTaskStatusValidator : AbstractValidator<UpdateTaskStatusCommand>
{
    public UpdateTaskStatusValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        
        RuleFor(x => x.NewStatus).NotEmpty()
            .Must(statusStr => Enum.TryParse<OrderStatus>(statusStr, true, out _))
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