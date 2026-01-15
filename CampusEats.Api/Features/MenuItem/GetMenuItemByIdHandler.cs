using CampusEats.Api.Infrastructure.Repositories;
using MediatR;

namespace CampusEats.Api.Features.MenuItem;

public record GetMenuItemByIdRequest(int Id) : IRequest<IResult>;

public class GetMenuItemByIdHandler(
    IMenuItemRepository menuItemRepository
) : IRequestHandler<GetMenuItemByIdRequest, IResult>
{
    public async Task<IResult> Handle(GetMenuItemByIdRequest request, CancellationToken cancellationToken)
    {
        var menuItem = await menuItemRepository.GetByIdAsync(request.Id);
        
        if (menuItem == null)
        {
            return Results.NotFound($"MenuItem with id {request.Id} not found");
        }

        return Results.Ok(menuItem);
    }
}
