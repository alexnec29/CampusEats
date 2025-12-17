using MediatR;

namespace CampusEats.Api.Features.Loyalty.GetLoyaltyTransactions;

public record GetLoyaltyTransactionsRequest(Guid UserId)
    : IRequest<IResult>;