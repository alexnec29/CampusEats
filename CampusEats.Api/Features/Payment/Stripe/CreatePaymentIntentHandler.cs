using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Utils.PaymentUtil;
using MediatR;

namespace CampusEats.Api.Features.Payment.Stripe;

public class CreatePaymentIntentHandler(
    PaymentProviderFactory paymentProviderFactory, 
    IMenuItemRepository menuItemRepository,
    IOrderRepository orderRepository
    ) : IRequestHandler<CreatePaymentIntentRequest, IResult>
{
    public async Task<IResult> Handle(CreatePaymentIntentRequest request, CancellationToken cancellationToken)
    {
        IPaymentService? provider = paymentProviderFactory.GetProvider(request.PaymentProvider);
        if (provider == null)
        {
            return Results.BadRequest($"Provider {request.PaymentProvider} is not a registered payment provider");
        }
        
        Models.Order? order = await orderRepository.GetByIdAsync(request.OrderId);
        if (order is null)
        {
            return Results.NotFound($"Order with id: {request.OrderId} not found");
        }
        
        decimal amount = 0;
        
        foreach (var cartItem in order.OrderItems)
        {
            Models.MenuItem? menuItem = await menuItemRepository.GetByIdAsync(cartItem.MenuItemId);
            if (menuItem == null)
            {
                return Results.NotFound($"Menu item with id: {cartItem.MenuItemId} not found");
            }
            amount += menuItem.Price * cartItem.Quantity;
        }

        string currency = "usd";
        int orderId = request.OrderId;
        
        string result = await provider.CreatePaymentIntentAsync(amount, currency, orderId);
        
        return Results.Ok(result);
    }
}