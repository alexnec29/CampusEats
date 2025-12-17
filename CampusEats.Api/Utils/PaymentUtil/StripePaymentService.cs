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
    
    public async Task<Dictionary<string, string>> CreatePaymentIntentAsync(decimal amount, string currency, int orderId)
    {
        var service = new PaymentIntentService();
        
        var paymentIntentCreateOptions = new PaymentIntentCreateOptions
        {
            Amount = (long) (amount * 100),
            Currency = currency,
            Metadata = new Dictionary<string, string>
            {
                {  "orderId", orderId.ToString() }
            }
        };

        var requestOptions = new RequestOptions()
        {
            IdempotencyKey = $"order_{orderId}_payment_intent"
        };
        
        var intent = await service.CreateAsync(paymentIntentCreateOptions, requestOptions);

        Dictionary<string, string> result = new Dictionary<string, string>
        {
            { "paymentIntentId", intent.Id },
            { "paymentIntentClientResult", intent.ClientSecret }
        };
            
        return result;
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
                EventTypes.PaymentIntentCreated => OrderStatus.PendingPayment,
                EventTypes.PaymentIntentPaymentFailed => OrderStatus.FailedPayment,
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

    public async Task<(bool, string)> CreateRefundAsync(string paymentIntentId)
    {
        var service = new RefundService();
        
        var refundCreateOptions = new RefundCreateOptions
        {
            PaymentIntent = paymentIntentId
        };
        
        var requestOptions = new RequestOptions()
        {
            IdempotencyKey = $"refund_payment_intent_{paymentIntentId}"
        };

        try
        {
            await service.CreateAsync(refundCreateOptions, requestOptions);
            return (true, "Refund successfully created");
        }
        catch (StripeException e)
        {
            logger.LogError("Error: {}", e);
            return (false, e.StripeError.Message);
        }
    }
}