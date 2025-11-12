using CampusEats.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Infrastructure.Repositories;

public class LoyaltyTransactionRepository : ILoyaltyTransactionRepository
{
    private readonly CampusEatsDbContext _context;

    public LoyaltyTransactionRepository(CampusEatsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(LoyaltyTransaction entity)
    {
        await _context.LoyaltyTransactions.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.LoyaltyTransactions.FindAsync(id);
        if (entity != null)
        {
            _context.LoyaltyTransactions.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IList<LoyaltyTransaction>> GetAllAsync() =>
        await _context.LoyaltyTransactions.ToListAsync();

    public async Task<LoyaltyTransaction?> GetByIdAsync(int id) =>
        await _context.LoyaltyTransactions.FindAsync(id);

    public async Task UpdateAsync(LoyaltyTransaction entity)
    {
        _context.LoyaltyTransactions.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IList<LoyaltyTransaction>> GetByAccountIdAsync(int accountId) =>
        await _context.LoyaltyTransactions
            .Where(t => t.LoyaltyAccountId == accountId)
            .ToListAsync();
}