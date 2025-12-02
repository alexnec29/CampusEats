using CampusEats.Api.Models.Enums;
using System.Text.Json.Serialization;
using MediatR;

namespace CampusEats.Api.Features.Order.UpdateOrderStatus;

public record UpdateOrderStatusRequest(
    [property: JsonIgnore] int OrderId,
    OrderStatus Status
) : IRequest<IResult>;