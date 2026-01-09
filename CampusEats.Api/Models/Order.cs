using CampusEats.Api.Models.Enums;
using Stripe;

namespace CampusEats.Api.Models;

public class Order
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public User User { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public string PaymentIntentId { get; set; } = string.Empty;
    public string PaymentProvider { get; set; } = string.Empty;
    public KitchenTask? KitchenTask { get; set; }
}