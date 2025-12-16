using MediatR;

namespace CampusEats.Api.Features.Loyalty.AdjustPoints;

public record AdjustPointsRequest(
    Guid UserId,
    int Points,
    string? Reason
) : IRequest<IResult>;