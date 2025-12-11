using Stripe.V2;

namespace CampusEats.Api.Utils.PaymentUtil;

public interface IPaymentService
{
    string Name { get; }
    Task<string> CreatePaymentIntentAsync(decimal amount, string currency, int orderId);
    Task ProcessWebhookAsync(HttpRequest request);
}