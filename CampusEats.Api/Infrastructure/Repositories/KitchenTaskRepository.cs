// Infrastructure/Repositories/KitchenTaskRepository.cs
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Infrastructure.Repositories;

public class KitchenTaskRepository : IKitchenTaskRepository
{
    private readonly CampusEatsDbContext _context;

    public KitchenTaskRepository(CampusEatsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(KitchenTask entity)
    {
        await _context.KitchenTasks.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.KitchenTasks.FindAsync(id);
        if (entity != null)
        {
            _context.KitchenTasks.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IList<KitchenTask>> GetAllAsync() =>
        await _context.KitchenTasks.ToListAsync();

    public async Task<KitchenTask?> GetByIdAsync(int id) =>
        await _context.KitchenTasks.FindAsync(id);

    public async Task UpdateAsync(KitchenTask entity)
    {
        _context.KitchenTasks.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IList<KitchenTask>> GetByStatusAsync(OrderStatus status) =>
        await _context.KitchenTasks.Where(t => t.Status == status).ToListAsync();

    public async Task<IList<KitchenTask>> GetByStaffIdAsync(int staffId) =>
        await _context.KitchenTasks
            .Where(t => t.AssignedStaffId == staffId)
            .ToListAsync();
}