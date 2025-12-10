using CampusEats.Api.Infrastructure.Repositories;
using MediatR;

namespace CampusEats.Api.Features.MenuItem;

public record GetAllMenuItemsRequest : IRequest<IResult>;

public class GetAllMenuItemsHandler : IRequestHandler<GetAllMenuItemsRequest, IResult>
{
    private readonly IMenuItemRepository _repository;

    public GetAllMenuItemsHandler(IMenuItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<IResult> Handle(GetAllMenuItemsRequest request, CancellationToken cancellationToken)
    {
        var items = await _repository.GetAllAsync();
        return Results.Ok(items);
    }
}
