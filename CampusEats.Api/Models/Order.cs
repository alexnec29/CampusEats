using CampusEats.Api.Models.Enums;

namespace CampusEats.Api.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public User User { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public Payment? Payment { get; set; }
    public KitchenTask? KitchenTask { get; set; }
}