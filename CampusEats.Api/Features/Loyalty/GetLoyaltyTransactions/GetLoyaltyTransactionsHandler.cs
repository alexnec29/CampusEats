using CampusEats.Api.Infrastructure.Repositories;
using MediatR;

namespace CampusEats.Api.Features.Loyalty.GetLoyaltyTransactions;

public class GetLoyaltyTransactionsHandler(
    IUserRepository userRepository,
    ILoyaltyAccountRepository loyaltyAccountRepository,
    ILoyaltyTransactionRepository loyaltyTransactionRepository,
    GetLoyaltyTransactionsValidator validator
) : IRequestHandler<GetLoyaltyTransactionsRequest, IResult>
{
    public async Task<IResult> Handle(
        GetLoyaltyTransactionsRequest request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(request, cancellationToken);

        var user = await userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            return Results.NotFound("User not found");

        var account = await loyaltyAccountRepository.GetByUserIdAsync(user.Id);
        if (account == null)
            return Results.NotFound("Loyalty account not found");

        var transactions =
            await loyaltyTransactionRepository.GetByAccountIdAsync(account.Id);

        var response = transactions
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                t.Id,
                t.Points,
                t.TransactionType,
                t.Description,
                t.CreatedAt
            });

        return Results.Ok(response);
    }
}