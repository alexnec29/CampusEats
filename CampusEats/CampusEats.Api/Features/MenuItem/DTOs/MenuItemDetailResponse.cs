using CampusEats.Api.Features.Allergen.DTOs;
namespace CampusEats.Api.Features.MenuItem.DTOs;

public class MenuItemDetailResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } 
    
    public ICollection<AllergenResponse> Allergens { get; set; } = new List<AllergenResponse>();
}