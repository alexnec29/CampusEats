using MediatR;

namespace CampusEats.Api.Features.Payment.Stripe;

public record CreatePaymentIntentRequest(
    string PaymentProvider, 
    int OrderId, 
    int? LoyaltyPointsToUse = null
) : IRequest<IResult>;