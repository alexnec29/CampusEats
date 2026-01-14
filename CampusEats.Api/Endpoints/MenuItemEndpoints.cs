using CampusEats.Api.Models.Enums;
using MediatR;

namespace CampusEats.Api.Features.MenuItem;

public static class MenuItemEndpoints
{
    public static void MapMenuItemEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/menu-items")
            .WithTags("MenuItems")
            .WithOpenApi();

        group.MapGet("/", async (IMediator mediator) =>
        {
            return await mediator.Send(new GetAllMenuItemsRequest());
        }).RequireAuthorization(nameof(AuthorizationPolicies.AllRolesPolicy));

        group.MapPost("/", async (CreateMenuItemRequest request, IMediator mediator) =>
        {
            return await mediator.Send(request);
        }).RequireAuthorization(nameof(AuthorizationPolicies.KitchenPolicy));

        group.MapDelete("/{id}", async (int id, IMediator mediator) =>
        {
            await mediator.Send(new DeleteMenuItemRequest(id));
            return Results.NoContent();
        }).RequireAuthorization(nameof(AuthorizationPolicies.KitchenPolicy));
    }
}
