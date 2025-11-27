using CampusEats.Api.Infrastructure.Repositories; 
using CampusEats.Api.Domain.Entities; 
using CampusEats.Api.Domain.Enums;
using MediatR;

namespace CampusEats.Api.Features.KitchenTask;

public record CreateKitchenTaskCommand(Guid OrderId) : IRequest<Result>;

public class CreateKitchenTaskHandler : IRequestHandler<CreateKitchenTaskCommand, Result>
{
    private readonly CampusEatsDbContext _context; 
    public CreateKitchenTaskHandler(CampusEatsDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(CreateKitchenTaskCommand request, CancellationToken ct)
    {
        var orderExists = await _context.Orders.AnyAsync(o => o.Id == request.OrderId, ct);
        if (!orderExists)
        {
            return Result.Failure("Order not found.");
        }

        var kitchenTask = new KitchenTask
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            Status = KitchenTaskStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.KitchenTasks.Add(kitchenTask);
        await _context.SaveChangesAsync(ct);
        
        return Result.Success();
    }
}