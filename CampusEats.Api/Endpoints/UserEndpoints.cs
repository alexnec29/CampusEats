using System.Security.Claims;
using CampusEats.Api.Features.User; 
using MediatR;

namespace CampusEats.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var users = app.MapGroup("api/user")
            .WithTags("Users")
            .WithOpenApi();

        // Register
        users.MapPost("/register", async (CreateUserRequest request, IMediator mediator) =>
            await mediator.Send(request)).AllowAnonymous();

        // Login
        users.MapPost("/login",
            async (HttpContext httpContext, LoginUserRequest request, IMediator mediator) =>
            {
                if (httpContext.User.Identity?.IsAuthenticated == true) 
                    return Results.Ok(new { message = "User already logged in" });
                
                return await mediator.Send(request);
            }).AllowAnonymous();

        users.MapPost("/logout", async (HttpContext httpContext, IMediator mediator) =>
        {
            if (!httpContext.Request.Cookies.TryGetValue("JWT", out var jwt))
            {
                return Results.Ok(); 
            }

            LogoutUserRequest request = new LogoutUserRequest(jwt);
            return await mediator.Send(request);
        }).RequireAuthorization("AllRoles");

        users.MapGet("/check-auth", async (IMediator mediator) => 
            await mediator.Send(new CheckAuthRequest())
        ).AllowAnonymous();

        users.MapPut("/update-buyer-profile", async (HttpContext httpContext, UpdateBuyerProfileRequest request, IMediator mediator) =>
        {
            Guid userId = new Guid(httpContext.User.FindFirstValue("/id") ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            request = request with { UserId = userId };
            return await mediator.Send(request);
        }).RequireAuthorization("Buyer");

        users.MapPut("/update-kitchen-profile",
            async (HttpContext httpContext, UpdateKitchenProfileRequest request, IMediator mediator) =>
            {
                Guid userId = new Guid(httpContext.User.FindFirstValue("/id") ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                request = request with { UserId = userId };
                return await mediator.Send(request);
            }).RequireAuthorization("Kitchen");

        users.MapGet("/buyer-profile", async (HttpContext httpContext, IMediator mediator) =>
        {
            Guid userId = new Guid(httpContext.User.FindFirstValue("/id") ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            GetBuyerProfileByUserIdRequest request = new GetBuyerProfileByUserIdRequest(userId);
            return await mediator.Send(request);
        }).RequireAuthorization("Buyer");
        
        users.MapGet("/kitchen-profile", async (HttpContext httpContext, IMediator mediator) =>
        {
            Guid userId = new Guid(httpContext.User.FindFirstValue("/id") ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            GetKitchenProfileByUserIdRequest request = new GetKitchenProfileByUserIdRequest(userId);
            return await mediator.Send(request);
        }).RequireAuthorization("Kitchen");
        
        users.MapGet("", async (HttpContext httpContext, IMediator mediator) =>
        {
            Guid userId = new Guid(httpContext.User.FindFirstValue("/id") ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            GetUserByIdRequest request = new GetUserByIdRequest(userId);
            return await mediator.Send(request);
        }).RequireAuthorization("AllRoles");
    }
}