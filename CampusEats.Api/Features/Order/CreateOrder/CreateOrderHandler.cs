using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using MediatR;

namespace CampusEats.Api.Features.Order.CreateOrder;

public class CreateOrderHandler(
    IOrderRepository orderRepository,
    IUserRepository userRepository,
    CreateOrderValidator validator
) : IRequestHandler<CreateOrderRequest, IResult>
{
    public async Task<IResult> Handle(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(request, cancellationToken);
    
        var user = await userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return Results.BadRequest(new { Message = "User does not exist", UserId = request.UserId });
        }
        
        var existingOrders = await orderRepository.GetOrdersByUserAsync(request.UserId);
        if (existingOrders.Any(o => o.Status == OrderStatus.Pending))
            return Results.Conflict("User already has a pending order");

        var order = new Models.Order
        {
            UserId = request.UserId,
            Notes = request.Notes,
            Status = OrderStatus.Pending,
            TotalAmount = 0m,
            KitchenTask = new Models.KitchenTask
            {
                Status = OrderStatus.Inactive
            }
        };

        await orderRepository.AddAsync(order);

        return Results.Created();
    }
}