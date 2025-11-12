using CampusEats.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Infrastructure.Repositories;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly CampusEatsDbContext _context;

    public OrderItemRepository(CampusEatsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(OrderItem entity)
    {
        await _context.OrderItems.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.OrderItems.FindAsync(id);
        if (entity != null)
        {
            _context.OrderItems.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IList<OrderItem>> GetAllAsync() =>
        await _context.OrderItems
            .Include(o => o.MenuItem)
            .Include(o => o.Order)
            .ToListAsync();

    public async Task<OrderItem?> GetByIdAsync(int id) =>
        await _context.OrderItems
            .Include(o => o.MenuItem)
            .Include(o => o.Order)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task UpdateAsync(OrderItem entity)
    {
        _context.OrderItems.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IList<OrderItem>> GetByOrderIdAsync(int orderId) =>
        await _context.OrderItems
            .Where(i => i.OrderId == orderId)
            .Include(i => i.MenuItem)
            .ToListAsync();
}