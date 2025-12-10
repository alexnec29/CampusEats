using CampusEats.Api.Models;

namespace CampusEats.Api.Infrastructure.Repositories;

public interface IBlackListTokenRepository : IRepository<Jwt, Guid>
{
    Task<Jwt?> GetByTextAsync(string text);
}