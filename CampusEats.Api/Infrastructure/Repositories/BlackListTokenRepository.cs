using CampusEats.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Infrastructure.Repositories;

public class BlackListTokenRepository(CampusEatsDbContext dbContext) : IBlackListTokenRepository
{
    
    public async Task AddAsync(Jwt jwt)
    {
        await dbContext.BlackListTokens.AddAsync(jwt);
        await dbContext.SaveChangesAsync();
    }

    public async Task<Jwt?> GetByIdAsync(Guid id)
    {
        return await dbContext.BlackListTokens.FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<IList<Jwt>> GetAllAsync()
    {
        return await dbContext.BlackListTokens.ToListAsync();
    }

    public async Task UpdateAsync(Jwt jwt)
    {
        dbContext.BlackListTokens.Update(jwt);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        Jwt? jwt = await dbContext.BlackListTokens.FirstOrDefaultAsync(j => j.Id == id);
        if (jwt != null)
        {
            dbContext.BlackListTokens.Remove(jwt);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<Jwt?> GetByTextAsync(string text)
    {
        return await dbContext.BlackListTokens.FirstOrDefaultAsync(j => j.Text == text);
    }
}