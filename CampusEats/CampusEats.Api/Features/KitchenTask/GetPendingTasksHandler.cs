using CampusEats.Api.Features.KitchenTask.DTOs;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using MediatR;

namespace CampusEats.Api.Features.KitchenTask;

public record GetPendingTasksQuery() : IRequest<IResult>;

public class GetPendingTasksHandler : IRequestHandler<GetPendingTasksQuery, IResult>
{
    private readonly IKitchenTaskRepository _taskRepository;

    public GetPendingTasksHandler(IKitchenTaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<IResult> Handle(GetPendingTasksQuery request, CancellationToken cancellationToken)
    {
        var pendingStatuses = new[] { OrderStatus.Pending, OrderStatus.Preparing };

        var tasks = (await _taskRepository.GetAllAsync())
            .Where(t => pendingStatuses.Contains(t.Status))
            .OrderBy(t => t.CreatedAt)
            .Select(t => new KitchenTaskResponse
            {
                Id = t.Id,
                Status = t.Status,
                AssignedStaffId = t.AssignedStaffId,
                CreatedAt = t.CreatedAt,
                CompletedAt = t.CompletedAt
            })
            .ToList();

        return Results.Ok(tasks);
    }
}