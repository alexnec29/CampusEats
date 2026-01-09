using FluentValidation;

namespace CampusEats.Api.Features.Order.ApplyLoyaltyDiscount;

public class ApplyLoyaltyDiscountValidator : AbstractValidator<ApplyLoyaltyDiscountRequest>
{
    public ApplyLoyaltyDiscountValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0)
            .WithMessage("Order ID must be greater than 0");

        RuleFor(x => x.PointsToRedeem)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Points to redeem must be non-negative");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
    }
}
