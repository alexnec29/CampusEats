using System.Security.Claims;
using CampusEats.Api.Features.User;
using MediatR;

namespace CampusEats.Api.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var admin = app.MapGroup("api/admin")
            .WithTags("Admin")
            .RequireAuthorization("Admin")   // only admins can access everything here
            .WithOpenApi();

        admin.MapGet("/users", async (IMediator mediator) =>
        {
            return await mediator.Send(new GetAllUsersRequest());
        });
        
        admin.MapPut("/users/{id}/role", async (Guid id, UpdateUserRoleRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(request with { UserId = id });
            return result;
        });
    }
}