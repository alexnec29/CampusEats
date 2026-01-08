using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Validators;
using CampusEats.Api.Features.Loyalty.EarnPoints;
using MediatR;

namespace CampusEats.Api.Features.Order.UpdateOrderStatus;

public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusRequest, IResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly UpdateOrderStatusValidator _validator;
    private readonly IMediator _mediator;

    private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
    {
        { OrderStatus.Inactive, new[] { OrderStatus.Pending, OrderStatus.Cancelled } },
        { OrderStatus.Pending,   new[] { OrderStatus.Placed, OrderStatus.Cancelled } },
        { OrderStatus.Placed,    new[] { OrderStatus.Preparing, OrderStatus.Cancelled, OrderStatus.Paid } },
        { OrderStatus.Paid,      new[] { OrderStatus.Preparing, OrderStatus.Cancelled } },
        { OrderStatus.Preparing, new[] { OrderStatus.Ready, OrderStatus.Cancelled } },
        { OrderStatus.Ready,     new[] { OrderStatus.Completed } },
    };

    public UpdateOrderStatusHandler(IOrderRepository orderRepository, UpdateOrderStatusValidator validator, IMediator mediator)
    {
        _orderRepository = orderRepository;
        _validator = validator;
        _mediator = mediator;
    }

    public async Task<IResult> Handle(UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAsync(request, cancellationToken);

        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order == null)
            return Results.NotFound();

        if (!AllowedTransitions.TryGetValue(order.Status, out var allowed))
            return Results.BadRequest($"Cannot update status from {order.Status}");

        if (!allowed.Contains(request.Status))
            return Results.BadRequest($"Invalid status transition: {order.Status} → {request.Status}");

        order.Status = request.Status;

        if (order.KitchenTask != null)
            order.KitchenTask.Status = request.Status;

        await _orderRepository.UpdateAsync(order);

        // Award loyalty points when order is completed
        if (request.Status == OrderStatus.Completed && order.TotalAmount > 0)
        {
            await _mediator.Send(new EarnPointsRequest(
                order.UserId,
                order.Id,
                order.TotalAmount
            ), cancellationToken);
        }

        return Results.Ok();
    }
}