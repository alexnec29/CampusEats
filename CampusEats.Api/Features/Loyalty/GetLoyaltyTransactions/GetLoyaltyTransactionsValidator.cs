using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentValidation;

namespace CampusEats.Api.Features.Loyalty.GetLoyaltyTransactions;

public class GetLoyaltyTransactionsValidator
    : AbstractValidator<GetLoyaltyTransactionsRequest>
{
    public GetLoyaltyTransactionsValidator(
        IUserRepository userRepository,
        ILoyaltyAccountRepository loyaltyAccountRepository)
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.")
            .MustAsync(async (userId, _) =>
            {
                var user = await userRepository.GetByIdAsync(userId);
                if (user == null || user.Role != Role.Buyer)
                    return false;

                var account = await loyaltyAccountRepository.GetByUserIdAsync(userId);
                return account != null;
            })
            .WithMessage("Loyalty account does not exist for this user.");
    }
}