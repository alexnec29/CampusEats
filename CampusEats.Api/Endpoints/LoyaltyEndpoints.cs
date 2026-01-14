using MediatR;
using System.Security.Claims;
using CampusEats.Api.Features.Loyalty.GetLoyaltyAccount;
using CampusEats.Api.Features.Loyalty.GetLoyaltyTransactions;
using CampusEats.Api.Features.Loyalty.RedeemPoints;
using CampusEats.Api.Features.Loyalty.AdjustPoints;
using CampusEats.Api.Models.Enums;

namespace CampusEats.Api.Endpoints;

public static class LoyaltyEndpoints
{
    public static void MapLoyaltyEndpoints(this WebApplication app)
    {
        var loyalty = app.MapGroup("api/loyalty")
            .WithTags("Loyalty")
            .WithOpenApi();

        // Get user's loyalty account
        loyalty.MapGet("/account", async (HttpContext httpContext, IMediator mediator) =>
        {
            var userId = new Guid(httpContext.User.FindFirstValue("/id")!);
            return await mediator.Send(new GetLoyaltyAccountRequest(userId));
        }).RequireAuthorization(nameof(AuthorizationPolicies.BuyerPolicy));

        // Get transaction history
        loyalty.MapGet("/transactions", async (HttpContext httpContext, IMediator mediator) =>
        {
            var userId = new Guid(httpContext.User.FindFirstValue("/id")!);
            return await mediator.Send(new GetLoyaltyTransactionsRequest(userId));
        }).RequireAuthorization(nameof(AuthorizationPolicies.BuyerPolicy));

        // Redeem points for discount
        loyalty.MapPost("/redeem", async (RedeemPointsRequest request, HttpContext httpContext, IMediator mediator) =>
        {
            var userId = new Guid(httpContext.User.FindFirstValue("/id")!);
            request = request with { UserId = userId };
            return await mediator.Send(request);
        }).RequireAuthorization(nameof(AuthorizationPolicies.BuyerPolicy));

        // Admin: Manually adjust points
        loyalty.MapPost("/adjust", async (AdjustPointsRequest request, IMediator mediator) =>
        {
            return await mediator.Send(request);
        }).RequireAuthorization(nameof(AuthorizationPolicies.AdminPolicy));
    }
}