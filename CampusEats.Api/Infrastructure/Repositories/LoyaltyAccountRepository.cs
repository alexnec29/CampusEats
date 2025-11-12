using CampusEats.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Infrastructure.Repositories;

public class LoyaltyAccountRepository : ILoyaltyAccountRepository
{
    private readonly CampusEatsDbContext _context;

    public LoyaltyAccountRepository(CampusEatsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(LoyaltyAccount entity)
    {
        await _context.LoyaltyAccounts.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.LoyaltyAccounts.FindAsync(id);
        if (entity != null)
        {
            _context.LoyaltyAccounts.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IList<LoyaltyAccount>> GetAllAsync() =>
        await _context.LoyaltyAccounts.Include(l => l.Transactions).ToListAsync();

    public async Task<LoyaltyAccount?> GetByIdAsync(int id) =>
        await _context.LoyaltyAccounts
            .Include(l => l.Transactions)
            .FirstOrDefaultAsync(l => l.Id == id);

    public async Task UpdateAsync(LoyaltyAccount entity)
    {
        _context.LoyaltyAccounts.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<LoyaltyAccount?> GetByUserIdAsync(int userId) =>
        await _context.LoyaltyAccounts
            .Include(l => l.Transactions)
            .FirstOrDefaultAsync(l => l.UserId == userId);
}