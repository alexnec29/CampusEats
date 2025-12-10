using CampusEats.Api.Models;

namespace CampusEats.Api.Infrastructure.Repositories;

public interface ILoyaltyTransactionRepository : IRepository<LoyaltyTransaction, int>
{
    Task<IList<LoyaltyTransaction>> GetByAccountIdAsync(int accountId);
}