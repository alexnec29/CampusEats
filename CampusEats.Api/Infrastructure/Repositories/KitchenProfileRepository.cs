using CampusEats.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Infrastructure.Repositories;

public class KitchenProfileRepository(CampusEatsDbContext dbContext) : IKitchenProfileRepository
{
    public async Task AddAsync(KitchenProfile kitchenProfile)
    {
        await dbContext.KitchenProfiles.AddAsync(kitchenProfile);
        await dbContext.SaveChangesAsync();
    }

    public async Task<KitchenProfile?> GetByIdAsync(Guid id)
    {
        return await dbContext.KitchenProfiles.FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<IList<KitchenProfile>> GetAllAsync()
    {
        return await dbContext.KitchenProfiles.ToListAsync();
    }

    public async Task UpdateAsync(KitchenProfile kitchenProfile)
    {
        dbContext.KitchenProfiles.Update(kitchenProfile);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        KitchenProfile? kitchenProfile = await dbContext.KitchenProfiles.FirstOrDefaultAsync(k => k.Id == id);
        if (kitchenProfile != null)
        {
            dbContext.KitchenProfiles.Remove(kitchenProfile);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<KitchenProfile?> GetByUserIdAsync(Guid userId)
    {
        return await dbContext.KitchenProfiles.FirstOrDefaultAsync(k => k.UserId == userId);
    }
}