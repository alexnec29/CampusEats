using MediatR;

namespace CampusEats.Api.Features.Payment;

public record CreatePaymentIntentRequest(string PaymentProvider, int OrderId) : IRequest<IResult>;