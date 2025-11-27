using MediatR;

namespace CampusEats.Api.Features.Order.RemoveOrderItem;

public record RemoveOrderItemRequest(
    int OrderId,
    int OrderItemId
) : IRequest<IResult>;