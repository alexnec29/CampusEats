using CampusEats.Api.Infrastructure.Repositories;
using FluentValidation;

namespace CampusEats.Api.Features.Order.CreateOrder;

public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator(IMenuItemRepository menuItemRepository)
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}