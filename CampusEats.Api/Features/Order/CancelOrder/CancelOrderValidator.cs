using FluentValidation;

namespace CampusEats.Api.Features.Order.CancelOrder;

public class CancelOrderValidator : AbstractValidator<CancelOrderRequest>
{
    public CancelOrderValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0)
            .WithMessage("OrderId must be greater than 0.");
    }
}