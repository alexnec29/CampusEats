using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;

namespace CampusEats.API.Infrastructure.Repositories;

// Here you can add specific queries for Users
public interface IUserRepository : IRepository<User, Guid>
{
    Task<User?> GetByUsernameAsync(string username);
}