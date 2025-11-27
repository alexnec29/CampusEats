using CampusEats.Api.Features.User;
using MediatR;

namespace CampusEats.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var users = app.MapGroup("/user")
            .WithTags("Users")
            .WithOpenApi();

        users.MapPost("/api/user/register", async (CreateUserRequest request, IMediator mediator) =>
            await mediator.Send(request)).AllowAnonymous();

        users.MapPost("/api/user/login",
            async (HttpContext httpContext, LoginUserRequest request, IMediator mediator) =>
            {
                if (httpContext.User.Identity?.IsAuthenticated == true) return Results.Ok("User already logged in");
                return await mediator.Send(request);
            }).AllowAnonymous();

        users.MapPost("/api/user/logout", async (LogoutUserRequest request, IMediator mediator) =>
            await mediator.Send(request)).RequireAuthorization("AllRoles");

        users.MapPut("/api/user/update-buyer-profile", async (UpdateBuyerProfileRequest request, IMediator mediator) =>
            await mediator.Send(request)).RequireAuthorization("Buyer");

        users.MapPut("/api/user/update-kitchen-profile",
            async (UpdateKitchenProfileRequest request, IMediator mediator) =>
                await mediator.Send(request)).RequireAuthorization("Kitchen");
    }
}