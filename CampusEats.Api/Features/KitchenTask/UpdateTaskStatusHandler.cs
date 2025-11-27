using MediatR;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;

namespace CampusEats.Api.Features.KitchenTask;

public record UpdateTaskStatusCommand(int TaskId, string NewStatus) : IRequest<IResult>;

public class UpdateTaskStatusHandler(
    IKitchenTaskRepository taskRepository,
    IOrderRepository orderRepository
) : IRequestHandler<UpdateTaskStatusCommand, IResult>
{
    public async Task<IResult> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.TaskId);
        if (task == null)
            return Results.NotFound("Kitchen task not found.");

        // Validate status string → enum
        if (!Enum.TryParse<OrderStatus>(request.NewStatus, true, out var newStatus))
            return Results.BadRequest("Invalid status value.");

        // Update kitchen task status
        task.Status = newStatus;

        // If completed, update completed date + update order status
        if (newStatus == OrderStatus.Completed)
        {
            task.CompletedAt = DateTime.UtcNow;

            var order = await orderRepository.GetByIdAsync(task.OrderId);
            if (order != null)
            {
                order.Status = OrderStatus.Ready;
                await orderRepository.UpdateAsync(order);
            }
        }

        await taskRepository.UpdateAsync(task);

        return Results.Ok(task);
    }
}