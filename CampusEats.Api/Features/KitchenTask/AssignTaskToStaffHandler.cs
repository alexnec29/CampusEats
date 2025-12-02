using MediatR;
using CampusEats.Api.Infrastructure.Repositories;

namespace CampusEats.Api.Features.KitchenTask;

public record AssignTaskToStaffCommand(int TaskId, Guid StaffId) : IRequest<IResult>;

public class AssignTaskToStaffHandler(
    IKitchenTaskRepository taskRepository,
    IUserRepository userRepository
) : IRequestHandler<AssignTaskToStaffCommand, IResult>
{
    public async Task<IResult> Handle(AssignTaskToStaffCommand request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.TaskId);
        if (task == null)
            return Results.NotFound("Kitchen task not found.");

        var staff = await userRepository.GetByIdAsync(request.StaffId);
        if (staff == null)
            return Results.NotFound("Staff member not found.");

        task.AssignedStaffId = request.StaffId;

        if (task.Status == Models.Enums.OrderStatus.Pending)
        {
            task.Status = Models.Enums.OrderStatus.Preparing;
        }

        await taskRepository.UpdateAsync(task);

        return Results.Ok(task);
    }
}