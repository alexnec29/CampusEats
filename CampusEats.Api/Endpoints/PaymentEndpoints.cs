using CampusEats.Api.Features.Payment.Stripe;
using MediatR;

namespace CampusEats.Api.Endpoints;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this WebApplication app)
    {
        var payments = app.MapGroup("api/payments")
            .WithTags("Users")
            .WithOpenApi();

        payments.MapPost("/create-payment-intent/{provider}", async (string provider, CreatePaymentIntentRequest request, IMediator mediator) =>
        {
            request = request with { PaymentProvider = provider };
            return await mediator.Send(request);
        });

        payments.MapPost("/webhook/{provider}", async (string provider, HttpRequest httpRequest, IMediator mediator) =>
        {
            PaymentWebhookRequest request = new PaymentWebhookRequest(provider, httpRequest);
            return await mediator.Send(request);
        });

    }
}