namespace CampusEats.Api.Features.Order;

public class OrderItemResponse
{
    public int Id { get; set; }
    public int MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}