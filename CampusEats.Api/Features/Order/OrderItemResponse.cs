namespace CampusEats.Api.Features.Order;

public class OrderItemResponse
{
    public int Id { get; set; }
    public int MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? MenuItemDescription { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Subtotal { get; set; }
    public DateTime AddedAt { get; set; }
}