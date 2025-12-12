namespace CampusEats.Api.Utils.PaymentUtil;

public class PayPalPaymentService : IPaymentService
{
    public string Name { get; } = "Paypal";
    
    public Task<string> CreatePaymentIntentAsync(decimal amount, string currency, int orderId)
    {
        throw new NotImplementedException();
    }

    public Task ProcessWebhookAsync(HttpRequest request)
    {
        throw new NotImplementedException();
    }
}