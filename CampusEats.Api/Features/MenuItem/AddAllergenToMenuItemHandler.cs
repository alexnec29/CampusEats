using CampusEats.Api.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.MenuItem;

public record AddAllergenToMenuItemRequest(int MenuItemId, int AllergenId) : IRequest<IResult>;

public class AddAllergenToMenuItemHandler(
    CampusEatsDbContext dbContext
) : IRequestHandler<AddAllergenToMenuItemRequest, IResult>
{
    public async Task<IResult> Handle(AddAllergenToMenuItemRequest request, CancellationToken cancellationToken)
    {
        var menuItem = await dbContext.MenuItems
            .Include(m => m.Allergens)
            .FirstOrDefaultAsync(m => m.Id == request.MenuItemId, cancellationToken);
        
        if (menuItem == null)
        {
            return Results.NotFound($"MenuItem with id {request.MenuItemId} not found");
        }

        var allergen = await dbContext.Allergens.FindAsync(new object[] { request.AllergenId }, cancellationToken);
        
        if (allergen == null)
        {
            return Results.NotFound($"Allergen with id {request.AllergenId} not found");
        }

        if (menuItem.Allergens.Any(a => a.Id == request.AllergenId))
        {
            return Results.Conflict("Allergen already added to this menu item");
        }

        menuItem.Allergens.Add(allergen);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(menuItem);
    }
}
