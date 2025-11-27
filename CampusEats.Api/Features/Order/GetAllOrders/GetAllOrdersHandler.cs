using CampusEats.Api.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Order.GetAllOrders;

public class GetAllOrdersHandler(
    IOrderRepository orderRepository
) : IRequestHandler<GetAllOrdersRequest, IResult>
{
    public async Task<IResult> Handle(GetAllOrdersRequest request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetAllAsync();

        var response = orders.Select(o => new OrderResponse
        {
            Id = o.Id,
            UserId = o.UserId,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            OrderDate = o.OrderDate,
            Notes = o.Notes,
            Items = o.OrderItems.Select(oi => new OrderItemResponse
            {
                MenuItemId = oi.MenuItemId,
                Quantity = oi.Quantity,
                Price = oi.Price
            }).ToList()
        }).ToList();

        return Results.Ok(response);
    }

}