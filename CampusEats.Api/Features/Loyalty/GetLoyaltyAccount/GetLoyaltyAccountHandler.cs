using CampusEats.Api.Infrastructure.Repositories;
using MediatR;

namespace CampusEats.Api.Features.Loyalty.GetLoyaltyAccount;

public class GetLoyaltyAccountHandler(
    IUserRepository userRepository,
    ILoyaltyAccountRepository loyaltyAccountRepository,
    GetLoyaltyAccountValidator validator
) : IRequestHandler<GetLoyaltyAccountRequest, IResult>
{
    public async Task<IResult> Handle(GetLoyaltyAccountRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(request, cancellationToken);

        var user = await userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            return Results.NotFound("User not found");

        var account = await loyaltyAccountRepository.GetByUserIdAsync(user.Id);

        if (account == null)
        {
            account = new Models.LoyaltyAccount
            {
                UserId = user.Id,
                PointsBalance = 0
            };

            await loyaltyAccountRepository.AddAsync(account);
        }

        return Results.Ok(new
        {
            account.Id,
            account.PointsBalance,
            account.CreatedAt
        });
    }
}