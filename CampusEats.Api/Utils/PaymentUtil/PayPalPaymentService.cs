namespace CampusEats.Api.Utils.PaymentUtil;

public class PayPalPaymentService : IPaymentService
{
    public string Name { get; } = "Paypal";
    
    public Task<Dictionary<string, string>> CreatePaymentIntentAsync(decimal amount, string currency, int orderId, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task ProcessWebhookAsync(HttpRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<(bool, string)> CreateRefundAsync(string paymentIntentId)
    {
        throw new NotImplementedException();
    }
}