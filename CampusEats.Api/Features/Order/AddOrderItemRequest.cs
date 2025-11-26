namespace CampusEats.Api.Features.Order;

public class AddOrderItemRequest
{
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
}