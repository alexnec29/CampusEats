using CampusEats.Api.Infrastructure.Repositories;
using MediatR;

namespace CampusEats.Api.Features.Order.GetOrdersByStatus;

public class GetOrdersByStatusHandler(
    IOrderRepository orderRepository
) : IRequestHandler<GetOrdersByStatusRequest, IResult>
{
    public async Task<IResult> Handle(GetOrdersByStatusRequest request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetOrdersByStatusAsync(request.Status);

        // Return entities directly to match the structure expected by frontend (same as GetUserOrders)
        return Results.Ok(orders);
    }
}
