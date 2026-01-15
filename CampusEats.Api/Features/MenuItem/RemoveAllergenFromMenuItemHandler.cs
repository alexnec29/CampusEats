using CampusEats.Api.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.MenuItem;

public record RemoveAllergenFromMenuItemRequest(int MenuItemId, int AllergenId) : IRequest<IResult>;

public class RemoveAllergenFromMenuItemHandler(
    CampusEatsDbContext dbContext
) : IRequestHandler<RemoveAllergenFromMenuItemRequest, IResult>
{
    public async Task<IResult> Handle(RemoveAllergenFromMenuItemRequest request, CancellationToken cancellationToken)
    {
        var menuItem = await dbContext.MenuItems
            .Include(m => m.Allergens)
            .FirstOrDefaultAsync(m => m.Id == request.MenuItemId, cancellationToken);
        
        if (menuItem == null)
        {
            return Results.NotFound($"MenuItem with id {request.MenuItemId} not found");
        }

        var allergen = menuItem.Allergens.FirstOrDefault(a => a.Id == request.AllergenId);
        
        if (allergen == null)
        {
            return Results.NotFound($"Allergen with id {request.AllergenId} not found on this menu item");
        }

        menuItem.Allergens.Remove(allergen);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}
