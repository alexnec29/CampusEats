using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Infrastructure.Repositories;

public class MenuItemRepository : IMenuItemRepository
{
    private readonly CampusEatsDbContext _context;

    public MenuItemRepository(CampusEatsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(MenuItem entity)
    {
        await _context.MenuItems.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.MenuItems.FindAsync(id);
        if (entity != null)
        {
            _context.MenuItems.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IList<MenuItem>> GetAllAsync() =>
        await _context.MenuItems.ToListAsync();

    public async Task<MenuItem?> GetByIdAsync(int id) =>
        await _context.MenuItems.FindAsync(id);

    public async Task UpdateAsync(MenuItem entity)
    {
        _context.MenuItems.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IList<MenuItem>> GetAvailableMenuItemsAsync() =>
        await _context.MenuItems.Where(m => m.IsAvailable).ToListAsync();

    public async Task<IList<MenuItem>> GetMenuItemsByCategoryAsync(MenuCategory category) =>
        await _context.MenuItems.Where(m => m.Category == category).ToListAsync();
}