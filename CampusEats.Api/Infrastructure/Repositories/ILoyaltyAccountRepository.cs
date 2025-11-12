using CampusEats.Api.Models;

namespace CampusEats.Api.Infrastructure.Repositories;

public interface ILoyaltyAccountRepository : IRepository<LoyaltyAccount, int>
{
    Task<LoyaltyAccount?> GetByUserIdAsync(int userId);
}