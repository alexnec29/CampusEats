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
        }).RequireAuthorization("AllRoles");

        users.MapPut("/update-buyer-profile", async (UpdateBuyerProfileRequest request, IMediator mediator) =>
            await mediator.Send(request)).RequireAuthorization("Buyer");

        users.MapPut("/update-kitchen-profile",
            async (UpdateKitchenProfileRequest request, IMediator mediator) =>
                await mediator.Send(request)).RequireAuthorization("Kitchen");
    }
}