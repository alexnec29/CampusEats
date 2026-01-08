using MediatR;

namespace CampusEats.Api.Features.Loyalty.EarnPoints;

public record EarnPointsRequest(
    Guid UserId,
    int OrderId,
    decimal OrderAmount
) : IRequest<IResult>;
