using MediatR;

namespace CampusEats.Api.Features.User;

public record GetKitchenProfileByUserIdRequest(Guid Id) : IRequest<IResult>;