using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Features.KitchenTask;
using MediatR;

namespace CampusEats.Api.Features.Order.GetOrderById;

public class GetOrderByIdHandler(
    IOrderRepository orderRepository
) : IRequestHandler<GetOrderByIdRequest, IResult>
{
    public async Task<IResult> Handle(GetOrderByIdRequest request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId);
        if (order == null) return Results.NotFound();

        var response = new OrderDetailResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            OrderDate = order.OrderDate,
            Notes = order.Notes,
            Items = order.OrderItems.Select(oi => new OrderDetailItemResponse
            {
                MenuItemId = oi.MenuItemId,
                Quantity = oi.Quantity,
                MenuItemPrice = oi.Price,
            }).ToList(),
            KitchenTask = order.KitchenTask != null ? new KitchenTaskResponse
            {
                Status = order.KitchenTask.Status,
                AssignedStaffId = order.KitchenTask.AssignedStaffId
            } : null
        };

        return Results.Ok(response);
    }

}