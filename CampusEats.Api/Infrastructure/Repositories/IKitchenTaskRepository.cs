using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;

namespace CampusEats.Api.Infrastructure.Repositories;

public interface IKitchenTaskRepository : IRepository<KitchenTask, int>
{
    Task<IList<KitchenTask>> GetByStatusAsync(OrderStatus status);
    Task<IList<KitchenTask>> GetByStaffIdAsync(int staffId);
}