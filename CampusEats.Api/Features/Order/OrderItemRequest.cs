// Features/Order/DTOs/OrderItemRequest.cs
namespace CampusEats.Api.Features.Order.DTOs;

public record OrderItemRequest(Guid MenuItemId, int Quantity);