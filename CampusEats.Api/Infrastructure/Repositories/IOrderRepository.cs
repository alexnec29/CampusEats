using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;

namespace CampusEats.Api.Infrastructure.Repositories;

public interface IOrderRepository : IRepository<Order, int>
{
    Task<IList<Order>> GetOrdersByUserAsync(int userId);
    Task<IList<Order>> GetOrdersByStatusAsync(OrderStatus status);
}