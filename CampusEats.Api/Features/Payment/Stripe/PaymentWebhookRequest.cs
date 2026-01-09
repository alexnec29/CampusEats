using MediatR;

namespace CampusEats.Api.Features.Payment.Stripe;

public record PaymentWebhookRequest(string PaymentProvider, HttpRequest HttpRequest) : IRequest<IResult>;