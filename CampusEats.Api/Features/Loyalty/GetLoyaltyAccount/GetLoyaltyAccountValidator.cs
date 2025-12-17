using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using FluentValidation;

namespace CampusEats.Api.Features.Loyalty.GetLoyaltyAccount;

public class GetLoyaltyAccountValidator : AbstractValidator<GetLoyaltyAccountRequest>
{
    public GetLoyaltyAccountValidator(IUserRepository userRepository)
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.")
            .MustAsync(async (id, _) =>
            {
                var user = await userRepository.GetByIdAsync(id);
                return user != null && user.Role == Role.Buyer;
            })
            .WithMessage("Loyalty accounts are only available for buyers.");
    }
}