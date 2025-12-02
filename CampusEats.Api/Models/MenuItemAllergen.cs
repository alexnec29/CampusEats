namespace CampusEats.Api.Models;

public class MenuItemAllergen
{
    public int Id { get; set; }
    public int MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;

    public int AllergenId { get; set; }
    public Allergen Allergen { get; set; } = null!;
}