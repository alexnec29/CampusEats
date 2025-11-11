namespace CampusEats.Api.Models;

public class Allergen
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<MenuItemAllergen> MenuItemAllergens { get; set; } = new List<MenuItemAllergen>();
}