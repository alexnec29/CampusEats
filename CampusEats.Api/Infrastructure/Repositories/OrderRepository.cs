using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly CampusEatsDbContext _context;

    public OrderRepository(CampusEatsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Order entity)
    {
        await _context.Orders.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Orders.FindAsync(id);
        if (entity != null)
        {
            _context.Orders.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IList<Order>> GetAllAsync() =>
        await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .Include(o => o.Payment)
            .Include(o => o.KitchenTask)
            .Include(o => o.User)
            .ToListAsync();

    public async Task<Order?> GetByIdAsync(int id) =>
        await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .Include(o => o.Payment)
            .Include(o => o.KitchenTask)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == id);

    // FIX: userId is now Guid
    public async Task<IList<Order>> GetOrdersByUserAsync(Guid userId) =>
        await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .Include(o => o.Payment)
            .Include(o => o.KitchenTask)
            .Include(o => o.User)
            .ToListAsync();

    public async Task<IList<Order>> GetOrdersByStatusAsync(OrderStatus status) =>
        await _context.Orders
            .Where(o => o.Status == status)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .Include(o => o.Payment)
            .Include(o => o.KitchenTask)
            .Include(o => o.User)
            .ToListAsync();

    public async Task UpdateAsync(Order entity)
    {
        _context.Orders.Update(entity);
        await _context.SaveChangesAsync();
    }
}