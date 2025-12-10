using MediatR;

namespace CampusEats.Api.Features.Order.GetUserOrders;

public record GetUserOrdersRequest(Guid UserId) : IRequest<IResult>;