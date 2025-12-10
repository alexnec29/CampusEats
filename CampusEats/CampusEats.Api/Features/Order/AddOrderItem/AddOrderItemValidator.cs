using CampusEats.Api.Infrastructure.Repositories;
using FluentValidation;

namespace CampusEats.Api.Features.Order.AddOrderItem;

public class AddOrderItemValidator : AbstractValidator<AddOrderItemRequest>
{
    public AddOrderItemValidator(IMenuItemRepository menuItemRepository)
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required.");

        RuleFor(x => x.MenuItemId)
            .NotEmpty().WithMessage("MenuItemId is required.")
            .MustAsync(async (id, _) => (await menuItemRepository.GetByIdAsync(id)) != null)
            .WithMessage("Menu item does not exist.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1.");
    }
}