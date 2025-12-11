using CampusEats.Api.Models.Enums;

namespace CampusEats.Api.Features.Order;

public class OrderResponse
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
    public string? Notes { get; set; }
    public int ItemCount { get; set; }
    public decimal EstimatedDeliveryTime { get; set; }
    public string? DeliveryAddress { get; set; }
    public List<OrderItemResponse> OrderItems { get; set; } = new();
    public string? PaymentMethod { get; set; }
    public string? KitchenStatus { get; set; }
}