using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using Stripe;

namespace CampusEats.Api.Utils.PaymentUtil;

public class StripePaymentService(
    IConfiguration configuration, 
    IOrderRepository ordersRepository,
    ILogger<StripePaymentService> logger
    ) : IPaymentService
{
    public string Name { get; } = "Stripe";
    
    public async Task<string> CreatePaymentIntentAsync(decimal amount, string currency, int orderId)
    {
        var service = new PaymentIntentService();
        
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long) (amount * 100),
            Currency = currency,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
            },
            Metadata = new Dictionary<string, string>
            {
                {  "orderId", orderId.ToString() }
            }
        };
        
        var intent = await service.CreateAsync(options);

        return intent.ClientSecret;
    }

    public async Task ProcessWebhookAsync(HttpRequest request)
{
    try
    {
        var json = await new StreamReader(request.Body).ReadToEndAsync();
        var signature = request.Headers["Stripe-Signature"].ToString();

        var stripeEvent = EventUtility.ConstructEvent(
            json,
            signature,
            configuration["Stripe:WebHookSecretKey"]
        );
        
        if (stripeEvent.Data.Object is not PaymentIntent paymentIntent)
        {
            return;
        }
        
        if (!paymentIntent.Metadata.TryGetValue("orderId", out var orderIdString) || 
            !int.TryParse(orderIdString, out var orderId))
        {
            return;
        }
        
        Order? order = await ordersRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            return;
        }

        order.Status = stripeEvent.Type switch
        {
            EventTypes.PaymentIntentSucceeded => OrderStatus.Paid,
            EventTypes.PaymentIntentCreated => OrderStatus.Placed,
            _ => OrderStatus.Cancelled
        };

        logger.LogInformation($"Payment intent: {stripeEvent.Type}, success: true");
        await ordersRepository.UpdateAsync(order);
        logger.LogInformation($"Order status: {order.Status}");
    }
    catch (Exception ex)
    {
        logger.LogError(ex.Message);
    }
}
}