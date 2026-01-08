using CampusEats.Api.Infrastructure;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Utils.PaymentUtil;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CampusEats.Api.Features.Payment.Stripe;

public class CreatePaymentIntentHandler(
    PaymentProviderFactory paymentProviderFactory, 
    IMenuItemRepository menuItemRepository,
    IOrderRepository orderRepository,
    ILoyaltyAccountRepository loyaltyAccountRepository,
    ILoyaltyTransactionRepository loyaltyTransactionRepository,
    CampusEatsDbContext dbContext,
    IConfiguration configuration
    ) : IRequestHandler<CreatePaymentIntentRequest, IResult>
{
    public async Task<IResult> Handle(CreatePaymentIntentRequest request, CancellationToken cancellationToken)
    {
        IPaymentService? provider = paymentProviderFactory.GetProvider(request.PaymentProvider);
        if (provider == null)
        {
            return Results.BadRequest($"Provider {request.PaymentProvider} is not a registered payment provider");
        }
        
        Models.Order? order = await orderRepository.GetByIdAsync(request.OrderId);
        if (order is null)
        {
            return Results.NotFound($"Order with id: {request.OrderId} not found");
        }
        
        decimal amount = 0;
        
        foreach (var cartItem in order.OrderItems)
        {
            Models.MenuItem? menuItem = await menuItemRepository.GetByIdAsync(cartItem.MenuItemId);
            if (menuItem == null)
            {
                return Results.NotFound($"Menu item with id: {cartItem.MenuItemId} not found");
            }
            amount += menuItem.Price * cartItem.Quantity;
        }

        // Apply loyalty discount if points are being used
        decimal loyaltyDiscount = 0;
        if (request.LoyaltyPointsToUse.HasValue && request.LoyaltyPointsToUse.Value > 0)
        {
            // Get account and validate points atomically
            var account = await dbContext.LoyaltyAccounts
                .FirstOrDefaultAsync(l => l.UserId == order.UserId, cancellationToken);
                
            if (account == null || account.PointsBalance < request.LoyaltyPointsToUse.Value)
            {
                return Results.BadRequest("Insufficient loyalty points");
            }

            // Calculate discount: $0.01 per point (configurable)
            var dollarsPerPoint = configuration.GetValue<decimal>("Loyalty:DollarsPerPoint", 0.01m);
            loyaltyDiscount = request.LoyaltyPointsToUse.Value * dollarsPerPoint;
            
            // Discount cannot exceed the order amount
            if (loyaltyDiscount > amount)
            {
                loyaltyDiscount = amount;
            }

            // Deduct points from loyalty account
            account.PointsBalance -= request.LoyaltyPointsToUse.Value;
            account.UpdatedAt = DateTime.UtcNow;

            var loyaltyTransaction = new Models.LoyaltyTransaction
            {
                LoyaltyAccountId = account.Id,
                Points = -request.LoyaltyPointsToUse.Value,
                TransactionType = "Redeem",
                Description = $"Applied to order #{order.Id}"
            };

            dbContext.LoyaltyTransactions.Add(loyaltyTransaction);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        // Calculate final amount after discount
        decimal finalAmount = amount - loyaltyDiscount;
        if (finalAmount < 0) finalAmount = 0;

        const string currency = "usd";
        int orderId = request.OrderId;
        
        var paymentIntentData = await provider.CreatePaymentIntentAsync(finalAmount, currency, orderId);

        paymentIntentData.TryGetValue("paymentIntentClientResult", out var clientResult);
        paymentIntentData.TryGetValue("paymentIntentId", out var paymentIntentId);

        if (clientResult == null || paymentIntentId == null)
        {
            return Results.InternalServerError($"A problem occured while creating payment intent, clientResult: {clientResult}, paymentIntentId: {paymentIntentId}");
        }
        
        order.PaymentIntentId = paymentIntentId;
        order.PaymentProvider = request.PaymentProvider;
        order.Status = OrderStatus.PendingPayment;
        order.TotalAmount = amount; // Store original amount before discount
        
        await orderRepository.UpdateAsync(order);
        
        return Results.Ok(new
        {
            ClientSecret = clientResult,
            OriginalAmount = amount,
            LoyaltyDiscount = loyaltyDiscount,
            FinalAmount = finalAmount,
            PointsUsed = request.LoyaltyPointsToUse ?? 0
        });
    }
}
