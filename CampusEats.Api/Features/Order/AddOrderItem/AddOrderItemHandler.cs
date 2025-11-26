using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using MediatR;

namespace CampusEats.Api.Features.Order.AddOrderItem;

public class AddOrderItemHandler(
    IOrderRepository orderRepository,
    IOrderItemRepository orderItemRepository,
    IMenuItemRepository menuItemRepository,
    AddOrderItemValidator validator
) : IRequestHandler<AddOrderItemRequest, IResult>
{
    public async Task<IResult> Handle(AddOrderItemRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(request, cancellationToken);

        var order = await orderRepository.GetByIdAsync(request.OrderId);
        if (order == null)
            return Results.NotFound("Order not found");

        if (order.Status != OrderStatus.Pending)
            return Results.BadRequest("Cannot add items to a non-pending order");

        var menuItem = await menuItemRepository.GetByIdAsync(request.MenuItemId);
        if (menuItem == null)
            return Results.NotFound("Menu item not found");

        var orderItem = new Models.OrderItem
        {
            OrderId = order.Id,
            MenuItemId = request.MenuItemId,
            Quantity = request.Quantity,
            Price = menuItem.Price
        };

        order.OrderItems.Add(orderItem);
        
        order.TotalAmount = order.OrderItems.Sum(i => i.Price * i.Quantity);

        await orderRepository.UpdateAsync(order);

        return Results.Ok();
    }
}