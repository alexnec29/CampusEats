using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using MediatR;

namespace CampusEats.Api.Features.MenuItem;

public record GetMenuItemsByCategoryRequest(MenuCategory Category) : IRequest<IResult>;

public class GetMenuItemsByCategoryHandler(
    IMenuItemRepository menuItemRepository
) : IRequestHandler<GetMenuItemsByCategoryRequest, IResult>
{
    public async Task<IResult> Handle(GetMenuItemsByCategoryRequest request, CancellationToken cancellationToken)
    {
        var menuItems = await menuItemRepository.GetMenuItemsByCategoryAsync(request.Category);
        
        return Results.Ok(menuItems);
    }
}
