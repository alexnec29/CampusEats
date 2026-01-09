using Stripe.V2;

namespace CampusEats.Api.Utils.PaymentUtil;

public interface IPaymentService
{
    string Name { get; }
    Task<Dictionary<string, string>> CreatePaymentIntentAsync(decimal amount, string currency, int orderId, Guid userId);
    Task ProcessWebhookAsync(HttpRequest request);
    Task<(bool, string)> CreateRefundAsync(string paymentIntentId);
}