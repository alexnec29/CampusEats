using CampusEats.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Infrastructure.Repositories;

public class AllergenRepository : IAllergenRepository
{
    private readonly CampusEatsDbContext _context;

    public AllergenRepository(CampusEatsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Allergen entity)
    {
        await _context.Allergens.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Allergens.FindAsync(id);
        if (entity != null)
        {
            _context.Allergens.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IList<Allergen>> GetAllAsync() =>
        await _context.Allergens.ToListAsync();

    public async Task<Allergen?> GetByIdAsync(int id) =>
        await _context.Allergens.FindAsync(id);

    public async Task UpdateAsync(Allergen entity)
    {
        _context.Allergens.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IList<Allergen>> GetAllergensForMenuItemAsync(int menuItemId) =>
        await _context.MenuItemAllergens
            .Where(ma => ma.MenuItemId == menuItemId)
            .Select(ma => ma.Allergen)
            .ToListAsync();
}