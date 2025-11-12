using CampusEats.Api.Models;

namespace CampusEats.Api.Infrastructure.Repositories;

public interface IOrderItemRepository : IRepository<OrderItem, int>
{
    Task<IList<OrderItem>> GetByOrderIdAsync(int orderId);
}