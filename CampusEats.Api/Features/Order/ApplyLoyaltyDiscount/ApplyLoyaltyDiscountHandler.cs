using CampusEats.Api.Infrastructure;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using MediatR;

namespace CampusEats.Api.Features.Order.ApplyLoyaltyDiscount;

public class ApplyLoyaltyDiscountHandler(
    IOrderRepository orderRepository,
    ILoyaltyAccountRepository loyaltyAccountRepository,
    ILoyaltyTransactionRepository loyaltyTransactionRepository,
    CampusEatsDbContext dbContext,
    ApplyLoyaltyDiscountValidator validator
) : IRequestHandler<ApplyLoyaltyDiscountRequest, IResult>
{
    // Conversion rate: 100 points = $1 discount
    private const decimal PointsToMoneyRate = 0.01m;

    public async Task<IResult> Handle(
        ApplyLoyaltyDiscountRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var order = await orderRepository.GetByIdAsync(request.OrderId);
        if (order == null)
        {
            return Results.NotFound("Order not found");
        }

        // Verify the order belongs to the user
        if (order.UserId != request.UserId)
        {
            return Results.Forbid();
        }

        // Only allow applying discount to Pending orders (cart)
        if (order.Status != OrderStatus.Pending)
        {
            return Results.BadRequest("Can only apply loyalty discount to pending orders");
        }

        // Get loyalty account
        var loyaltyAccount = await loyaltyAccountRepository.GetByUserIdAsync(request.UserId);
        if (loyaltyAccount == null)
        {
            return Results.NotFound("Loyalty account not found");
        }

        // Validate sufficient points
        if (loyaltyAccount.PointsBalance < request.PointsToRedeem)
        {
            return Results.BadRequest("Insufficient loyalty points");
        }

        // Calculate the original total from order items
        decimal originalTotal = order.OrderItems.Sum(item => item.Price * item.Quantity);

        // Calculate discount amount
        decimal discountAmount = request.PointsToRedeem * PointsToMoneyRate;

        // Ensure discount doesn't exceed order total
        int actualPointsUsed = request.PointsToRedeem;
        if (discountAmount > originalTotal)
        {
            discountAmount = originalTotal;
            // Recalculate actual points used based on capped discount
            // Use Floor to ensure we never exceed available points
            actualPointsUsed = (int)Math.Floor(discountAmount / PointsToMoneyRate);
        }

        // Calculate new total
        decimal newTotal = originalTotal - discountAmount;

        // Update order with discount information
        order.LoyaltyPointsUsed = actualPointsUsed;
        order.DiscountAmount = discountAmount;
        order.TotalAmount = newTotal;

        // Deduct points from loyalty account
        loyaltyAccount.PointsBalance -= actualPointsUsed;
        loyaltyAccount.UpdatedAt = DateTime.UtcNow;

        // Create loyalty transaction record
        var transaction = new LoyaltyTransaction
        {
            LoyaltyAccountId = loyaltyAccount.Id,
            Points = -actualPointsUsed,
            TransactionType = "Redeem",
            Description = $"Redeemed for order #{order.Id} discount"
        };

        // Wrap database operations in a transaction to ensure consistency
        await using var dbTransaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await loyaltyTransactionRepository.AddAsync(transaction);
            await loyaltyAccountRepository.UpdateAsync(loyaltyAccount);
            await orderRepository.UpdateAsync(order);
            
            await dbTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            throw;
        }

        return Results.Ok(new
        {
            orderId = order.Id,
            pointsUsed = actualPointsUsed,
            discountAmount = discountAmount,
            originalTotal = originalTotal,
            newTotal = newTotal,
            remainingPoints = loyaltyAccount.PointsBalance
        });
    }
}
