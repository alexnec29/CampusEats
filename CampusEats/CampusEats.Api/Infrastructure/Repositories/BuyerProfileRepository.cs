using CampusEats.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Infrastructure.Repositories;

public class BuyerProfileRepository(CampusEatsDbContext dbContext) : IBuyerProfileRepository
{
    public async Task AddAsync(BuyerProfile buyerProfile)
    {
        await dbContext.BuyerProfiles.AddAsync(buyerProfile);
        await dbContext.SaveChangesAsync();
    }

    public async Task<BuyerProfile?> GetByIdAsync(Guid id)
    {
        return await dbContext.BuyerProfiles.FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IList<BuyerProfile>> GetAllAsync()
    {
        return await dbContext.BuyerProfiles.ToListAsync();
    }

    public async Task UpdateAsync(BuyerProfile buyerProfile)
    {
        dbContext.BuyerProfiles.Update(buyerProfile);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        BuyerProfile? buyerProfile = await dbContext.BuyerProfiles.FirstOrDefaultAsync(b => b.Id == id);
        if (buyerProfile != null)
        {
            dbContext.BuyerProfiles.Remove(buyerProfile);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<BuyerProfile?> GetByUserIdAsync(Guid userId)
    {
        return await dbContext.BuyerProfiles.FirstOrDefaultAsync(b => b.UserId == userId);
    }
}