using MediatR;

namespace CampusEats.Api.Features.Loyalty.GetLoyaltyAccount;

public record GetLoyaltyAccountRequest(Guid UserId)
    : IRequest<IResult>;