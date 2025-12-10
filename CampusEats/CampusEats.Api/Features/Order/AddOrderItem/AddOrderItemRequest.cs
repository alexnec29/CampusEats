using MediatR;
using System.Text.Json.Serialization;

namespace CampusEats.Api.Features.Order.AddOrderItem;

public record AddOrderItemRequest(
    [property: JsonIgnore] int OrderId,
    int MenuItemId,
    int Quantity
) : IRequest<IResult>;