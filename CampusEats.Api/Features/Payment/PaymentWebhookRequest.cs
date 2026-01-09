using MediatR;

namespace CampusEats.Api.Features.Payment;

public record PaymentWebhookRequest(string PaymentProvider, HttpRequest HttpRequest) : IRequest<IResult>;