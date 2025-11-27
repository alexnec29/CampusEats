using MediatR;

namespace CampusEats.Api.Features.Order.GetOrderById;

public record GetOrderByIdRequest(int OrderId) : IRequest<IResult>;