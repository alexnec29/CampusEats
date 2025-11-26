using CampusEats.Api.Features.OrderItem;
using CampusEats.Api.Models.Enums;

namespace CampusEats.Api.Features.Order;

public class OrderResponse
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime OrderDate { get; set; }
    public string? Notes { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
}