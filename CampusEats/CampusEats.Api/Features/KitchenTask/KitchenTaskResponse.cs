using CampusEats.Api.Models.Enums;

namespace CampusEats.Api.Features.KitchenTask;

public class KitchenTaskResponse
{
    public int Id { get; set; }
    public OrderStatus Status { get; set; }
    public Guid? AssignedStaffId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}