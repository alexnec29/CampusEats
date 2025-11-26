using CampusEats.Api.Models.Enums;

namespace CampusEats.Api.Features.Order;

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}