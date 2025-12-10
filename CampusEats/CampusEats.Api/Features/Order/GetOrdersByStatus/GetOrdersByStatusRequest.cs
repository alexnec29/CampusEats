using CampusEats.Api.Models.Enums;
using MediatR;

namespace CampusEats.Api.Features.Order.GetOrdersByStatus;

public record GetOrdersByStatusRequest(OrderStatus Status) : IRequest<IResult>;