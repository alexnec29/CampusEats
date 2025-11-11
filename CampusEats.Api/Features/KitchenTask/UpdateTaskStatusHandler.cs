using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.KitchenTask;
public record UpdateTaskStatusCommand(Guid TaskId, string NewStatus) : IRequest<Result>;

public class UpdateTaskStatusHandler : IRequestHandler<UpdateTaskStatusCommand, Result>
{
    private readonly CampusEatsDbContext _context;

    public UpdateTaskStatusHandler(CampusEatsDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateTaskStatusCommand request, CancellationToken ct)
    {
        var task = await _context.KitchenTasks
            .Include(t => t.Order) 
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, ct);

        if (task == null)
            return Result.Failure("KitchenTask not found.");
        
        if (!Enum.TryParse<KitchenTaskStatus>(request.NewStatus, true, out var newStatus))
            return Result.Failure("Invalid status value.");

        task.Status = newStatus;
        
        if (newStatus == KitchenTaskStatus.Completed)
        {
            task.CompletedAt = DateTime.UtcNow;
            if (task.Order != null)
            {
                task.Order.Status = OrderStatus.ReadyForPickup; 
            }
        }

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}