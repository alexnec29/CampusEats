using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentValidation;

namespace CampusEats.Api.Features.Loyalty.AdjustPoints;

public class AdjustPointsValidator : AbstractValidator<AdjustPointsRequest>
{
    public AdjustPointsValidator(
        IUserRepository userRepository,
        ILoyaltyAccountRepository loyaltyAccountRepository)
    {
        RuleFor(x => x.Points)
            .NotEqual(0).WithMessage("Points adjustment cannot be zero.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .MustAsync(async (userId, _) =>
            {
                var user = await userRepository.GetByIdAsync(userId);
                if (user == null || user.Role != Role.Buyer)
                    return false;

                var account = await loyaltyAccountRepository.GetByUserIdAsync(userId);
                return account != null;
            })
            .WithMessage("Invalid buyer or loyalty account.");
    }
}