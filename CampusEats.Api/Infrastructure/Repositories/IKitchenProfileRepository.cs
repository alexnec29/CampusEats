using CampusEats.Api.Models;

namespace CampusEats.Api.Infrastructure.Repositories;

public interface IKitchenProfileRepository : IRepository<KitchenProfile, Guid>
{
    Task<KitchenProfile?> GetByUserIdAsync(Guid userId);
}