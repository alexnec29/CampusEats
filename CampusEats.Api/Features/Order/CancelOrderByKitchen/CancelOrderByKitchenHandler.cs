using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Utils.PaymentUtil;
using MediatR;

namespace CampusEats.Api.Features.Order.CancelOrderByKitchen;

public class CancelOrderByKitchenHandler(
    IOrderRepository orderRepository,
    PaymentProviderFactory paymentProviderFactory
    ) : IRequestHandler<CancelOrderByKitchenRequest, IResult>
{
    public async Task<IResult> Handle(CancelOrderByKitchenRequest request, CancellationToken cancellationToken)
    {
        Models.Order? order = await orderRepository.GetByIdAsync(request.OrderId);
        if (order == null)
        {
            return Results.NotFound($"The order with id: {request.OrderId} was not found");
        }

        var provider = paymentProviderFactory.GetProvider(order.PaymentProvider);

        if (provider == null)
        {
            throw new InvalidOperationException(
                $"Payment provider '{order.PaymentProvider}' could not be resolved via the Factory.");
        }
        
        var result = await provider.CreateRefundAsync(order.PaymentIntentId);

        if (result.Item1)
        {
            order.Status = OrderStatus.Cancelled;
            await orderRepository.UpdateAsync(order);
            return Results.Ok(result.Item2);
        }
        
        return Results.UnprocessableEntity(result.Item2);
    }
}