using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentValidation;

namespace CampusEats.Api.Features.Loyalty.RedeemPoints;

public class RedeemPointsValidator : AbstractValidator<RedeemPointsRequest>
{
    public RedeemPointsValidator(
        IUserRepository userRepository,
        ILoyaltyAccountRepository loyaltyAccountRepository)
    {
        RuleFor(x => x.Points)
            .GreaterThan(0).WithMessage("Points must be greater than zero.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .MustAsync(async (userId, _) =>
            {
                var user = await userRepository.GetByIdAsync(userId);
                if (user == null || user.Role != Role.Buyer)
                    return false;

                var account = await loyaltyAccountRepository.GetByUserIdAsync(userId);
                return account != null && account.PointsBalance >= 0;
            })
            .WithMessage("Invalid loyalty account or user.");
    }
}