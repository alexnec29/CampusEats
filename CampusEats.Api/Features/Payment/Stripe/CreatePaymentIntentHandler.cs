using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Utils.PaymentUtil;
using MediatR;

namespace CampusEats.Api.Features.Payment.Stripe;

public class CreatePaymentIntentHandler(
    PaymentProviderFactory paymentProviderFactory, 
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
        
        // Use the order's TotalAmount which already includes loyalty points discount
        decimal amount = order.TotalAmount;

        const string currency = "usd";
        int orderId = request.OrderId;
        
        var paymentIntentData = await provider.CreatePaymentIntentAsync(amount, currency, orderId);

        paymentIntentData.TryGetValue("paymentIntentClientResult", out var clientResult);
        paymentIntentData.TryGetValue("paymentIntentId", out var paymentIntentId);

        if (clientResult == null || paymentIntentId == null)
        {
            return Results.InternalServerError($"A problem occured while creating payment intent, clientResult: {clientResult}, paymentIntentId: {paymentIntentId}");
        }
        
        order.PaymentIntentId = paymentIntentId;
        order.PaymentProvider = request.PaymentProvider;
        order.Status = OrderStatus.PendingPayment;
        
        await orderRepository.UpdateAsync(order);
        
        return Results.Ok(clientResult);
    }
}