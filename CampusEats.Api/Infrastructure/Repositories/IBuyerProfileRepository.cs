using CampusEats.Api.Models;

namespace CampusEats.Api.Infrastructure.Repositories;

public interface IBuyerProfileRepository : IRepository<BuyerProfile, Guid>
{
    Task<BuyerProfile?> GetByUserIdAsync(Guid userId);
}