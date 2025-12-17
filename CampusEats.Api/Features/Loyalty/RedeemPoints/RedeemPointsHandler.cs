using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using MediatR;

namespace CampusEats.Api.Features.Loyalty.RedeemPoints;

public class RedeemPointsHandler(
    IUserRepository userRepository,
    ILoyaltyAccountRepository loyaltyAccountRepository,
    ILoyaltyTransactionRepository loyaltyTransactionRepository,
    RedeemPointsValidator validator
) : IRequestHandler<RedeemPointsRequest, IResult>
{
    public async Task<IResult> Handle(
        RedeemPointsRequest request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(request, cancellationToken);

        var user = await userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            return Results.NotFound("User not found");

        var account = await loyaltyAccountRepository.GetByUserIdAsync(user.Id);
        if (account == null)
            return Results.NotFound("Loyalty account not found");

        if (account.PointsBalance < request.Points)
            return Results.BadRequest("Insufficient loyalty points");

        account.PointsBalance -= request.Points;
        account.UpdatedAt = DateTime.UtcNow;

        var transaction = new LoyaltyTransaction
        {
            LoyaltyAccountId = account.Id,
            Points = -request.Points,
            TransactionType = "Redeem",
            Description = request.Description ?? "Points redeemed"
        };

        await loyaltyTransactionRepository.AddAsync(transaction);
        await loyaltyAccountRepository.UpdateAsync(account);

        return Results.Ok(new
        {
            account.PointsBalance
        });
    }
}