using MediatR;

namespace CampusEats.Api.Features.Order.ApplyLoyaltyDiscount;

public record ApplyLoyaltyDiscountRequest(
    int OrderId,
    int PointsToRedeem,
    Guid UserId
) : IRequest<IResult>;
