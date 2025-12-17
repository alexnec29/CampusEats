using MediatR;
using System.Text.Json.Serialization;

namespace CampusEats.Api.Features.Loyalty.RedeemPoints;

public record RedeemPointsRequest(
    [property: JsonIgnore] Guid UserId,
    int Points,
    string? Description
) : IRequest<IResult>;