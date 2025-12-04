using MediatR;

namespace CampusEats.Api.Features.User;

public record GetBuyerProfileByUserIdRequest(Guid Id) : IRequest<IResult>;