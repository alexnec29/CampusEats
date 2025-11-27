using MediatR;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;

namespace CampusEats.Api.Features.KitchenTask;

public record CreateKitchenTaskCommand(int OrderId) : IRequest<IResult>;

public class CreateKitchenTaskHandler(
    IKitchenTaskRepository kitchenTaskRepository,
    IOrderRepository orderRepository
) : IRequestHandler<CreateKitchenTaskCommand, IResult>
{
    public async Task<IResult> Handle(CreateKitchenTaskCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId);
        if (order == null)
            return Results.NotFound("Order not found.");

        // Prevent duplicate tasks
        var existingTask = await kitchenTaskRepository.GetByOrderIdAsync(request.OrderId);
        if (existingTask != null)
            return Results.BadRequest("Kitchen task already exists for this order.");

        var task = new Models.KitchenTask
        {
            OrderId = request.OrderId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await kitchenTaskRepository.AddAsync(task);

        return Results.Ok(task);
    }
}