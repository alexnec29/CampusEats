using CampusEats.Api.Models;
using MediatR;

namespace CampusEats.Api.Features.Payment.Stripe;

public record CreatePaymentIntentRequest(string PaymentProvider, int OrderId, int? LoyaltyPointsToRedeem = null) : IRequest<IResult>;