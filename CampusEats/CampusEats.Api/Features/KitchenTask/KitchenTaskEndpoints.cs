using CampusEats.Api.Features.KitchenTask.DTOs;
using MediatR;

namespace CampusEats.Api.Features.KitchenTask;

public static class KitchenTaskEndpoints
{
    public static void MapKitchenEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/kitchen")
            .WithTags("Kitchen")
            .WithOpenApi(); 

        // GET /api/kitchen/tasks (pending)
        group.MapGet("/tasks", async (IMediator mediator) =>
            {
                return await mediator.Send(new GetPendingTasksQuery());
            })
            .Produces<List<KitchenTaskResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/tasks/{id}/status", 
            async (int id, UpdateTaskStatusRequest request, IMediator mediator) =>
            {
                var command = new UpdateTaskStatusCommand(id, request.NewStatus);
                return await mediator.Send(command);
            });

        group.MapPut("/tasks/{id}/assign", 
            async (int id, AssignTaskRequest request, IMediator mediator) =>
            {
                var command = new AssignTaskToStaffCommand(id, request.StaffId);
                return await mediator.Send(command);
            });

    }
}