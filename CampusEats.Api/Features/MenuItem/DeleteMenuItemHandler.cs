using CampusEats.Api.Infrastructure.Repositories;
using MediatR;

namespace CampusEats.Api.Features.MenuItem;

public record DeleteMenuItemRequest(int Id) : IRequest;

public class DeleteMenuItemHandler : IRequestHandler<DeleteMenuItemRequest>
{
    private readonly IMenuItemRepository _repository;

    public DeleteMenuItemHandler(IMenuItemRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(DeleteMenuItemRequest request, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(request.Id);
    }
}
