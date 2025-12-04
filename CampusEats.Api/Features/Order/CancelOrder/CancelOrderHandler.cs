using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using MediatR;

namespace CampusEats.Api.Features.Order.CancelOrder;

public class CancelOrderHandler(IOrderRepository orderRepository, CancelOrderValidator validator)
    : IRequestHandler<CancelOrderRequest, IResult>
{
    public async Task<IResult> Handle(CancelOrderRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(request, cancellationToken);

        var order = await orderRepository.GetByIdAsync(request.OrderId);
        if (order == null)
            return Results.NotFound("Order not found");

        if (order.Status != OrderStatus.Pending)
            return Results.BadRequest("Only pending orders can be cancelled");
        
        order.Status = OrderStatus.Cancelled;
        if (order.KitchenTask != null)
        {
            order.KitchenTask.Status = OrderStatus.Cancelled;
            order.KitchenTask.CompletedAt = DateTime.UtcNow;
        }

        await orderRepository.UpdateAsync(order);

        return Results.Ok(order);
    }
}