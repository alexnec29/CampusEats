using AutoMapper;
using AutoMapper.QueryableExtensions;
using CampusEats.Api.Features.KitchenTask.DTOs;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.KitchenTask;

public record GetPendingTasksQuery() : IRequest<Result<List<KitchenTaskResponse>>>;

public class GetPendingTasksHandler : IRequestHandler<GetPendingTasksQuery, Result<List<KitchenTaskResponse>>>
{
    private readonly CampusEatsDbContext _context;
    private readonly IMapper _mapper;

    public GetPendingTasksHandler(CampusEatsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<KitchenTaskResponse>>> Handle(GetPendingTasksQuery request, CancellationToken ct)
    {
        var pendingStatuses = new[] { KitchenTaskStatus.Pending, KitchenTaskStatus.Preparing };

        var tasks = await _context.KitchenTasks
            .Where(t => pendingStatuses.Contains(t.Status))
            .OrderBy(t => t.CreatedAt) 
            .ProjectTo<KitchenTaskResponse>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return Result.Success(tasks);
    }
}