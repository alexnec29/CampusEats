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
            var result = await mediator.Send(new GetPendingTasksQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        })
        .Produces<List<KitchenTaskResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        // PUT /api/kitchen/tasks/{id}/status
        group.MapPut("/tasks/{id}/status", 
            async (Guid id, UpdateTaskStatusRequest request, IMediator mediator) =>
        {
            var command = new UpdateTaskStatusCommand(id, request.NewStatus);
            var result = await mediator.Send(command);
            
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        // PUT /api/kitchen/tasks/{id}/assign
        group.MapPut("/tasks/{id}/assign", 
            async (Guid id, AssignTaskRequest request, IMediator mediator) =>
        {
            var command = new AssignTaskToStaffCommand(id, request.StaffId);
            var result = await mediator.Send(command);

            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
        
    }
}