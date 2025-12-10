using MediatR;

namespace CampusEats.Api.Features.Order.CancelOrder;

public record CancelOrderRequest(int OrderId) : IRequest<IResult>;