using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.KitchenTask;

public record AssignTaskToStaffCommand(Guid TaskId, Guid StaffId) : IRequest<Result>;

public class AssignTaskToStaffHandler : IRequestHandler<AssignTaskToStaffCommand, Result>
{
    private readonly CampusEatsDbContext _context;

    public AssignTaskToStaffHandler(CampusEatsDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(AssignTaskToStaffCommand request, CancellationToken ct)
    {
        var task = await _context.KitchenTasks.FindAsync(new object[] { request.TaskId }, ct);
        if (task == null)
            return Result.Failure("KitchenTask not found.");
        
        var staffExists = await _context.Users.AnyAsync(u => u.Id == request.StaffId, ct);
        if (!staffExists)
            return Result.Failure("Staff member not found.");

        task.StaffId = request.StaffId;
        
        if (task.Status == KitchenTaskStatus.Pending)
        {
            task.Status = KitchenTaskStatus.Preparing;
        }

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}