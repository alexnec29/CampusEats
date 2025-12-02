using MediatR;

namespace CampusEats.Api.Features.Order.UpdateOrderItemQuantity;

public record UpdateOrderItemQuantityRequest(
    int OrderId,
    int OrderItemId,
    int Quantity
) : IRequest<IResult>;