using CampusEats.Api.Models.Enums;

namespace CampusEats.Api.Models;

public class KitchenTask
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public int? AssignedStaffId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public Order Order { get; set; } = null!;
    public User? AssignedStaff { get; set; }
}