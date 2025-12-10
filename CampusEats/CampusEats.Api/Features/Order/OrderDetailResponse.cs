using CampusEats.Api.Models.Enums;
using CampusEats.Api.Features.Payment;
using CampusEats.Api.Features.KitchenTask;

namespace CampusEats.Api.Features.Order;

public class OrderDetailResponse
{
    public int Id { get; set; }
    public Guid UserId { get; set; }

    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime OrderDate { get; set; }
    public string? Notes { get; set; }
    public List<OrderDetailItemResponse> Items { get; set; } = new();
    public PaymentInfoResponse? Payment { get; set; }
    public KitchenTaskResponse? KitchenTask { get; set; }
}