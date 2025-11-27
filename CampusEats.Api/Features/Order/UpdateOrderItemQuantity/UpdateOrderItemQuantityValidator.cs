using FluentValidation;

namespace CampusEats.Api.Features.Order.UpdateOrderItemQuantity;

public class UpdateOrderItemQuantityValidator : AbstractValidator<UpdateOrderItemQuantityRequest>
{
    public UpdateOrderItemQuantityValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
