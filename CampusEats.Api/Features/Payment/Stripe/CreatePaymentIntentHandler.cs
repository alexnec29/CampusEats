using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Utils.PaymentUtil;
using MediatR;

namespace CampusEats.Api.Features.Payment.Stripe;

public class CreatePaymentIntentHandler(
    PaymentProviderFactory paymentProviderFactory, 
    IMenuItemRepository menuItemRepository,
    IOrderRepository orderRepository,
    ILoyaltyAccountRepository loyaltyAccountRepository,
    ILoyaltyTransactionRepository loyaltyTransactionRepository
    ) : IRequestHandler<CreatePaymentIntentRequest, IResult>
{
    // Loyalty points conversion: 100 points = $1.00
    private const decimal PointsPerDollar = 100m;
    
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

        // Apply loyalty points discount if requested
        int pointsRedeemed = 0;
        if (request.LoyaltyPointsToRedeem.HasValue && request.LoyaltyPointsToRedeem.Value > 0)
        {
            var loyaltyAccount = await loyaltyAccountRepository.GetByUserIdAsync(order.UserId);
            if (loyaltyAccount == null)
            {
                return Results.BadRequest("Loyalty account not found");
            }

            if (loyaltyAccount.PointsBalance < request.LoyaltyPointsToRedeem.Value)
            {
                return Results.BadRequest($"Insufficient loyalty points. Available: {loyaltyAccount.PointsBalance}, Requested: {request.LoyaltyPointsToRedeem.Value}");
            }

            // Calculate discount: PointsPerDollar points = $1 discount
            decimal discount = request.LoyaltyPointsToRedeem.Value / PointsPerDollar;
            
            // Ensure discount doesn't exceed total amount
            if (discount > amount)
            {
                discount = amount;
            }

            amount -= discount;
            pointsRedeemed = request.LoyaltyPointsToRedeem.Value;

            // Deduct loyalty points immediately
            loyaltyAccount.PointsBalance -= pointsRedeemed;
            loyaltyAccount.UpdatedAt = DateTime.UtcNow;

            var transaction = new Models.LoyaltyTransaction
            {
                LoyaltyAccountId = loyaltyAccount.Id,
                Points = -pointsRedeemed,
                TransactionType = "Redeem",
                Description = $"Redeemed for order #{order.Id} discount",
                CreatedAt = DateTime.UtcNow
            };

            await loyaltyTransactionRepository.AddAsync(transaction);
            await loyaltyAccountRepository.UpdateAsync(loyaltyAccount);
        }

        const string currency = "usd";
        int orderId = request.OrderId;
        
        var paymentIntentData = await provider.CreatePaymentIntentAsync(amount, currency, orderId);

        paymentIntentData.TryGetValue("paymentIntentClientResult", out var clientResult);
        paymentIntentData.TryGetValue("paymentIntentId", out var paymentIntentId);

        if (clientResult == null || paymentIntentId == null)
        {
            return Results.InternalServerError($"A problem occured while creating payment intent, clientResult: {clientResult}, paymentIntentId: {paymentIntentId}");
        }
        
        order.PaymentIntentId = paymentIntentId;
        order.PaymentProvider = request.PaymentProvider;
        order.Status = OrderStatus.PendingPayment;
        
        await orderRepository.UpdateAsync(order);
        
        return Results.Ok(clientResult);
    }
}