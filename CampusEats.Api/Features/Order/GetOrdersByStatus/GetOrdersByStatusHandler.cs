using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Features.OrderItem;
using MediatR;

namespace CampusEats.Api.Features.Order.GetOrdersByStatus;

public class GetOrdersByStatusHandler(
    IOrderRepository orderRepository
) : IRequestHandler<GetOrdersByStatusRequest, IResult>
{
    public async Task<IResult> Handle(GetOrdersByStatusRequest request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetOrdersByStatusAsync(request.Status);

        if (!orders.Any())
            return Results.NotFound();

        // Map to DTO
        var response = orders.Select(o => new OrderDetailResponse
        {
            Id = o.Id,
            UserId = o.UserId,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            OrderDate = o.OrderDate,
            Notes = o.Notes,
            Items = o.OrderItems.Select(oi => new OrderDetailItemResponse
            {
                MenuItemId = oi.MenuItemId,
                Quantity = oi.Quantity,
                MenuItemPrice = oi.Price,
            }).ToList(),
        }).ToList();

        return Results.Ok(response);
    }
}
