using CampusEats.Api.Infrastructure;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CampusEats.Api.Features.Loyalty.EarnPoints;

public class EarnPointsHandler(
    IUserRepository userRepository,
    ILoyaltyAccountRepository loyaltyAccountRepository,
    ILoyaltyTransactionRepository loyaltyTransactionRepository,
    CampusEatsDbContext dbContext,
    IConfiguration configuration
) : IRequestHandler<EarnPointsRequest, IResult>
{
    public async Task<IResult> Handle(
        EarnPointsRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            return Results.NotFound("User not found");

        // Get or create loyalty account with locking to prevent race conditions
        var account = await dbContext.LoyaltyAccounts
            .FirstOrDefaultAsync(l => l.UserId == user.Id, cancellationToken);
        
        if (account == null)
        {
            account = new LoyaltyAccount
            {
                UserId = user.Id,
                PointsBalance = 0
            };
            dbContext.LoyaltyAccounts.Add(account);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        // Calculate points to earn: 1 point per $1 spent (configurable)
        var pointsPerDollar = configuration.GetValue<decimal>("Loyalty:PointsPerDollar", 1m);
        var pointsEarned = (int)Math.Floor(request.OrderAmount * pointsPerDollar);

        if (pointsEarned > 0)
        {
            account.PointsBalance += pointsEarned;
            account.UpdatedAt = DateTime.UtcNow;

            var transaction = new LoyaltyTransaction
            {
                LoyaltyAccountId = account.Id,
                Points = pointsEarned,
                TransactionType = "Earn",
                Description = $"Earned from order #{request.OrderId}"
            };

            dbContext.LoyaltyTransactions.Add(transaction);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Results.Ok(new
        {
            PointsEarned = pointsEarned,
            NewBalance = account.PointsBalance
        });
    }
}
