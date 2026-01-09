using CampusEats.Api.Utils.PaymentUtil;
using MediatR;

namespace CampusEats.Api.Features.Payment;

public class PaymentWebhookHandler(PaymentProviderFactory paymentProviderFactory) : IRequestHandler<PaymentWebhookRequest, IResult>
{
    public async Task<IResult> Handle(PaymentWebhookRequest request, CancellationToken cancellationToken)
    {
        IPaymentService? provider = paymentProviderFactory.GetProvider(request.PaymentProvider);
        if (provider == null)
        {
            return Results.BadRequest($"Provider {request.PaymentProvider} is not a registered payment provider");
        }

        await provider.ProcessWebhookAsync(request.HttpRequest);
        
        return Results.Ok();
    }
}