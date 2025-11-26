namespace CampusEats.Api.Features.Order;

public class CreateOrderRequest
{
    public Guid UserId { get; set; }
    public string? Notes { get; set; }
    public List<AddOrderItemRequest> Items { get; set; } = new();
}