using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using MediatR;

namespace CampusEats.Api.Features.Order.ApplyLoyaltyPoints;

public class ApplyLoyaltyPointsHandler(
    IOrderRepository orderRepository,
    ILoyaltyAccountRepository loyaltyAccountRepository,
    ILoyaltyTransactionRepository loyaltyTransactionRepository,
    IMenuItemRepository menuItemRepository
) : IRequestHandler<ApplyLoyaltyPointsRequest, IResult>
{
    private const decimal PointsToUsdConversionRate = 0.01m; // 1 point = $0.01

    public async Task<IResult> Handle(
        ApplyLoyaltyPointsRequest request,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId);
        if (order == null)
            return Results.NotFound("Order not found");

        if (order.UserId != request.UserId)
            return Results.Forbid();

        if (order.Status != OrderStatus.Pending)
            return Results.BadRequest("Can only apply loyalty points to pending orders");

        var loyaltyAccount = await loyaltyAccountRepository.GetByUserIdAsync(request.UserId);
        if (loyaltyAccount == null)
            return Results.NotFound("Loyalty account not found");

        if (request.Points < 0)
            return Results.BadRequest("Points must be non-negative");

        if (request.Points > loyaltyAccount.PointsBalance)
            return Results.BadRequest("Insufficient loyalty points");

        // Calculate order subtotal
        decimal subtotal = 0;
        foreach (var item in order.OrderItems)
        {
            var menuItem = await menuItemRepository.GetByIdAsync(item.MenuItemId);
            if (menuItem != null)
            {
                subtotal += menuItem.Price * item.Quantity;
            }
        }

        // Calculate discount amount
        decimal discountAmount = request.Points * PointsToUsdConversionRate;

        // Ensure discount doesn't exceed order total
        if (discountAmount > subtotal)
            discountAmount = subtotal;

        // Revert previous loyalty points if any
        if (order.RedeemedLoyaltyPoints > 0)
        {
            loyaltyAccount.PointsBalance += order.RedeemedLoyaltyPoints;
            
            // Create refund transaction (positive points)
            var previousTransaction = new LoyaltyTransaction
            {
                LoyaltyAccountId = loyaltyAccount.Id,
                Points = order.RedeemedLoyaltyPoints,
                TransactionType = "Refund",
                Description = $"Refund for order #{order.Id} - points reapplied"
            };
            await loyaltyTransactionRepository.AddAsync(previousTransaction);
        }

        // Apply new points
        int actualPointsToRedeem = (int)Math.Floor(Math.Min(discountAmount, subtotal) / PointsToUsdConversionRate);
        discountAmount = actualPointsToRedeem * PointsToUsdConversionRate;

        order.RedeemedLoyaltyPoints = actualPointsToRedeem;
        order.LoyaltyPointsDiscount = discountAmount;
        order.TotalAmount = Math.Max(0, subtotal - discountAmount);

        // Deduct points from loyalty account
        loyaltyAccount.PointsBalance -= actualPointsToRedeem;
        loyaltyAccount.UpdatedAt = DateTime.UtcNow;

        // Create loyalty transaction
        if (actualPointsToRedeem > 0)
        {
            var transaction = new LoyaltyTransaction
            {
                LoyaltyAccountId = loyaltyAccount.Id,
                Points = -actualPointsToRedeem,
                TransactionType = "Redeem",
                Description = $"Applied to order #{order.Id}"
            };
            await loyaltyTransactionRepository.AddAsync(transaction);
        }

        await loyaltyAccountRepository.UpdateAsync(loyaltyAccount);
        await orderRepository.UpdateAsync(order);

        return Results.Ok(new
        {
            order.Id,
            order.TotalAmount,
            order.LoyaltyPointsDiscount,
            order.RedeemedLoyaltyPoints,
            RemainingLoyaltyPoints = loyaltyAccount.PointsBalance
        });
    }
}
