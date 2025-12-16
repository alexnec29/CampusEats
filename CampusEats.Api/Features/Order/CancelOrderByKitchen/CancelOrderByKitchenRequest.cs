using MediatR;

namespace CampusEats.Api.Features.Order.CancelOrderByKitchen;

public record CancelOrderByKitchenRequest(int OrderId) : IRequest<IResult>;