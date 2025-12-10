using MediatR;

namespace CampusEats.Api.Features.Order.GetAllOrders;

public record GetAllOrdersRequest() : IRequest<IResult>;