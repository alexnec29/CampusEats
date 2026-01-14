using CampusEats.Api.Infrastructure.Repositories;
using MediatR;

namespace CampusEats.Api.Features.Order.RemoveOrderItem;

public class RemoveOrderItemHandler(
    IOrderRepository orderRepository
) : IRequestHandler<RemoveOrderItemRequest, IResult>
{
    public async Task<IResult> Handle(RemoveOrderItemRequest request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId);
        if (order == null)
            return Results.NotFound("Order not found");

        if (order.Status != Models.Enums.OrderStatus.Pending)
            return Results.BadRequest("Cannot remove items from a non-pending order");

        var item = order.OrderItems.FirstOrDefault(i => i.Id == request.OrderItemId);
        if (item == null)
            return Results.NotFound("Order item not found");

        order.OrderItems.Remove(item);

        RecalculateOrderTotal(order);

        await orderRepository.UpdateAsync(order);

        return Results.Ok();
    }
    private static void RecalculateOrderTotal(Models.Order order)
    {
        order.TotalAmount = order.OrderItems.Sum(i => i.Quantity * i.Price);
    }
}