using MediatR;
using System.Text.Json.Serialization;

namespace CampusEats.Api.Features.Order.ApplyLoyaltyPoints;

public record ApplyLoyaltyPointsRequest(
    [property: JsonIgnore] int OrderId,
    [property: JsonIgnore] Guid UserId,
    int Points
) : IRequest<IResult>;
