using CampusEats.Api.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.MenuItem;

public record SearchMenuItemsRequest(string? SearchTerm) : IRequest<IResult>;

public class SearchMenuItemsHandler(
    CampusEatsDbContext dbContext
) : IRequestHandler<SearchMenuItemsRequest, IResult>
{
    public async Task<IResult> Handle(SearchMenuItemsRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var allItems = await dbContext.MenuItems.ToListAsync(cancellationToken);
            return Results.Ok(allItems);
        }

        var searchTerm = request.SearchTerm.ToLower();
        var menuItems = await dbContext.MenuItems
            .Where(m => m.Name.ToLower().Contains(searchTerm) || 
                       (m.Description != null && m.Description.ToLower().Contains(searchTerm)))
            .ToListAsync(cancellationToken);
        
        return Results.Ok(menuItems);
    }
}
