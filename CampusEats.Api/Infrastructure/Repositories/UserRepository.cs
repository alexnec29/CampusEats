using CampusEats.API.Infrastructure;
using CampusEats.API.Infrastructure.Repositories;
using CampusEats.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Infrastructure.Repositories;

public class UserRepository(CampusEatsDbContext dbContext) : IUserRepository
{
    public async Task AddAsync(User user)
    {
        await dbContext.AddAsync(user);
        await dbContext.SaveChangesAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IList<User>> GetAllAsync()
    {
        return await dbContext.Users.ToListAsync();
    }

    public async Task UpdateAsync(User user)
    {
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        User? user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user != null)
        {
            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
    }
}