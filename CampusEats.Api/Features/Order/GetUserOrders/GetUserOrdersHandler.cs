using CampusEats.Api.Infrastructure.Repositories;
using MediatR;

namespace CampusEats.Api.Features.Order.GetUserOrders;

public class GetUserOrdersHandler(
    IOrderRepository orderRepository
) : IRequestHandler<GetUserOrdersRequest, IResult>
{
    public async Task<IResult> Handle(GetUserOrdersRequest request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetOrdersByUserAsync(request.UserId);
        if (!orders.Any())
            return Results.NotFound();

        return Results.Ok(orders);
    }
}