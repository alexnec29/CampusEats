using CampusEats.Api.Infrastructure.Repositories;
using MediatR;

namespace CampusEats.Api.Features.Order.UpdateOrderItemQuantity;

public class UpdateOrderItemQuantityHandler(
    IOrderRepository orderRepository,
    UpdateOrderItemQuantityValidator validator
) : IRequestHandler<UpdateOrderItemQuantityRequest, IResult>
{
    public async Task<IResult> Handle(UpdateOrderItemQuantityRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(request, cancellationToken);

        var order = await orderRepository.GetByIdAsync(request.OrderId);
        if (order == null)
            return Results.NotFound("Order not found");

        var item = order.OrderItems.FirstOrDefault(i => i.Id == request.OrderItemId);
        if (item == null)
            return Results.NotFound("Order item not found");

        item.Quantity = request.Quantity;

        RecalculateOrderTotal(order);

        await orderRepository.UpdateAsync(order);

        return Results.Ok();
    }

    private void RecalculateOrderTotal(Models.Order order)
    {
        order.TotalAmount = order.OrderItems.Sum(i => i.Quantity * i.Price);
    }
}
