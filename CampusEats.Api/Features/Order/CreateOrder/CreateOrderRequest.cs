using MediatR;

namespace CampusEats.Api.Features.Order.CreateOrder;

public record CreateOrderRequest(
    Guid UserId,
    string? Notes
) : IRequest<IResult>;