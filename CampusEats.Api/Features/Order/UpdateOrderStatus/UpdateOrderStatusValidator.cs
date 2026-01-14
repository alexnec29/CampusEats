using CampusEats.Api.Models.Enums;
using FluentValidation;

namespace CampusEats.Api.Features.Order.UpdateOrderStatus;

public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
    {
        { OrderStatus.Inactive, new[] { OrderStatus.Pending, OrderStatus.Cancelled } },
        { OrderStatus.Pending,   new[] { OrderStatus.Preparing, OrderStatus.Cancelled } },
        { OrderStatus.Preparing, new[] { OrderStatus.Ready, OrderStatus.Cancelled } },
        { OrderStatus.Ready,     new[] { OrderStatus.Completed } },
    };

    public UpdateOrderStatusValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status value.");
    }

    public static bool IsTransitionAllowed(OrderStatus current, OrderStatus next)
    {
        return AllowedTransitions.TryGetValue(current, out var allowed) && allowed.Contains(next);
    }
}