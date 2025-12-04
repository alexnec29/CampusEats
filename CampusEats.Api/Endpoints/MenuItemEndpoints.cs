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
        }).RequireAuthorization();

        group.MapPost("/", async (CreateMenuItemRequest request, IMediator mediator) =>
        {
            return await mediator.Send(request);
        }).RequireAuthorization("AllRoles"); // Allow all roles for testing purposes
    }
}
