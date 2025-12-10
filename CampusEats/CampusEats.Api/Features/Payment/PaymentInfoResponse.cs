using CampusEats.Api.Models.Enums;

namespace CampusEats.Api.Features.Payment;

public class PaymentInfoResponse
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public PaymentMethod Method { get; set; }
    public string? TransactionId { get; set; }
    public DateTime CreatedAt { get; set; }
}