using CampusEats.Api.Models;

namespace CampusEats.Api.Infrastructure.Repositories;

public interface IAllergenRepository : IRepository<Allergen, int>
{
    Task<IList<Allergen>> GetAllergensForMenuItemAsync(int menuItemId);
}