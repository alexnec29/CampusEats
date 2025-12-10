using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
namespace CampusEats.Api.Infrastructure.Repositories;

public interface IMenuItemRepository : IRepository<MenuItem, int>
{
    Task<IList<MenuItem>> GetAvailableMenuItemsAsync();
    Task<IList<MenuItem>> GetMenuItemsByCategoryAsync(MenuCategory category);
}