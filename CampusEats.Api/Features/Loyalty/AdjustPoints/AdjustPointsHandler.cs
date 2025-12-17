using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using MediatR;

namespace CampusEats.Api.Features.Loyalty.AdjustPoints;

public class AdjustPointsHandler(
    IUserRepository userRepository,
    ILoyaltyAccountRepository loyaltyAccountRepository,
    ILoyaltyTransactionRepository loyaltyTransactionRepository,
    AdjustPointsValidator validator
) : IRequestHandler<AdjustPointsRequest, IResult>
{
    public async Task<IResult> Handle(
        AdjustPointsRequest request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(request, cancellationToken);

        var user = await userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            return Results.NotFound("User not found");

        var account = await loyaltyAccountRepository.GetByUserIdAsync(user.Id);
        if (account == null)
            return Results.NotFound("Loyalty account not found");

        var newBalance = account.PointsBalance + request.Points;
        if (newBalance < 0)
            return Results.BadRequest("Adjustment would result in negative balance");

        account.PointsBalance = newBalance;
        account.UpdatedAt = DateTime.UtcNow;

        var transaction = new LoyaltyTransaction
        {
            LoyaltyAccountId = account.Id,
            Points = request.Points,
            TransactionType = "AdminAdjustment",
            Description = request.Reason ?? "Manual adjustment by admin"
        };

        await loyaltyTransactionRepository.AddAsync(transaction);
        await loyaltyAccountRepository.UpdateAsync(account);

        return Results.Ok(new
        {
            account.PointsBalance
        });
    }
}