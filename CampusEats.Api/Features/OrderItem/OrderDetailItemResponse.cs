namespace CampusEats.Api.Features.OrderItem;

public class OrderDetailItemResponse
{
    public int Id { get; set; }
    public int MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public string MenuItemDescription { get; set; } = string.Empty;
    public decimal MenuItemPrice { get; set; }
    public int Quantity { get; set; }
}