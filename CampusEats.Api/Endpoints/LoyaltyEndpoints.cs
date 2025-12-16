using MediatR;
using System.Security.Claims;
using CampusEats.Api.Features.Loyalty.GetLoyaltyAccount;
using CampusEats.Api.Features.Loyalty.GetLoyaltyTransactions;
using CampusEats.Api.Features.Loyalty.RedeemPoints;
using CampusEats.Api.Features.Loyalty.AdjustPoints;

public static class LoyaltyEndpoints
{
    public static void MapLoyaltyEndpoints(this WebApplication app)
    {
        var loyalty = app.MapGroup("api/loyalty")
            .WithTags("Loyalty")
            .RequireAuthorization("Buyer")
            .WithOpenApi();

        // Get user's loyalty account
        loyalty.MapGet("/account", async (HttpContext httpContext, IMediator mediator) =>
        {
            var userId = new Guid(httpContext.User.FindFirstValue("/id")!);
            return await mediator.Send(new GetLoyaltyAccountRequest(userId));
        });

        // Get transaction history
        loyalty.MapGet("/transactions", async (HttpContext httpContext, IMediator mediator) =>
        {
            var userId = new Guid(httpContext.User.FindFirstValue("/id")!);
            return await mediator.Send(new GetLoyaltyTransactionsRequest(userId));
        });

        // Redeem points for discount
        loyalty.MapPost("/redeem", async (RedeemPointsRequest request, HttpContext httpContext, IMediator mediator) =>
        {
            var userId = new Guid(httpContext.User.FindFirstValue("/id")!);
            request = request with { UserId = userId };
            return await mediator.Send(request);
        });

        // Admin: Manually adjust points
        loyalty.MapPost("/adjust", async (AdjustPointsRequest request, IMediator mediator) =>
        {
            return await mediator.Send(request);
        }).RequireAuthorization("Admin");
    }
}