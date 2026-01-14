using System.Security.Claims;
using CampusEats.Api.Features.User;
using CampusEats.Api.Models.Enums;
using MediatR;

namespace CampusEats.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var users = app.MapGroup("api/user")
            .WithTags("Users")
            .WithOpenApi();

        users.MapPost("/register", async (CreateUserRequest request, IMediator mediator) =>
            await mediator.Send(request)).AllowAnonymous();

        users.MapPost("/login",
            async (HttpContext httpContext, LoginUserRequest request, IMediator mediator) =>
            {
                if (httpContext.User.Identity?.IsAuthenticated == true) return Results.Ok("User already logged in");
                return await mediator.Send(request);
            }).AllowAnonymous();

        users.MapPost("/logout", async (HttpContext httpContext, IMediator mediator) =>
        {
            string jwt = httpContext.Request.Cookies["JWT"]!;
            LogoutUserRequest request = new LogoutUserRequest(jwt);
            return await mediator.Send(request);
        }).RequireAuthorization(nameof(AuthorizationPolicies.AllRolesPolicy));
        
        users.MapPut("/change-password", async (HttpContext httpContext, ChangePasswordRequest request, IMediator mediator) =>
        {
            Guid userId = new Guid(httpContext.User.FindFirstValue("/id")!);
            request = request with { UserId = userId };
            return await mediator.Send(request);
        }).RequireAuthorization(nameof(AuthorizationPolicies.AllRolesPolicy));

        users.MapPut("/update-buyer-profile", async (HttpContext httpContext, UpdateBuyerProfileRequest request, IMediator mediator) =>
        {
            Guid userId = new Guid(httpContext.User.FindFirstValue("/id")!);
            request = request with { UserId = userId };
            return await mediator.Send(request);
        }).RequireAuthorization(nameof(AuthorizationPolicies.BuyerPolicy));

        users.MapPut("/update-kitchen-profile",
            async (HttpContext httpContext, UpdateKitchenProfileRequest request, IMediator mediator) =>
            {
                Guid userId = new Guid(httpContext.User.FindFirstValue("/id")!);
                request = request with { UserId = userId };
                return await mediator.Send(request);
            }).RequireAuthorization(nameof(AuthorizationPolicies.KitchenPolicy));

        users.MapGet("/check-auth", (HttpContext httpContext) =>
        {
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                var role = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
                return Results.Ok(new { isAuthenticated = true, username = httpContext.User.Identity?.Name, role = role });
            }
            return Results.Ok(new { isAuthenticated = false });
        }).AllowAnonymous();

        users.MapGet("/buyer-profile", async (HttpContext httpContext, IMediator mediator) =>
        {
            Guid userId = new Guid(httpContext.User.FindFirstValue("/id")!);
            GetBuyerProfileByUserIdRequest request = new GetBuyerProfileByUserIdRequest(userId);
            return await mediator.Send(request);
        }).RequireAuthorization(nameof(AuthorizationPolicies.BuyerPolicy));
        
        users.MapGet("/kitchen-profile", async (HttpContext httpContext, IMediator mediator) =>
        {
            Guid userId = new Guid(httpContext.User.FindFirstValue("/id")!);
            GetKitchenProfileByUserIdRequest request = new GetKitchenProfileByUserIdRequest(userId);
            return await mediator.Send(request);
        }).RequireAuthorization(nameof(AuthorizationPolicies.KitchenPolicy));
        
        users.MapGet("", async (HttpContext httpContext, IMediator mediator) =>
        {
            Guid userId = new Guid(httpContext.User.FindFirstValue("/id")!);
            GetUserByIdRequest request = new GetUserByIdRequest(userId);
            return await mediator.Send(request);
        }).RequireAuthorization(nameof(AuthorizationPolicies.AllRolesPolicy));
    }
}