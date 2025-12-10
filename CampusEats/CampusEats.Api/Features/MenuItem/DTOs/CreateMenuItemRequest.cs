using System.ComponentModel.DataAnnotations;
namespace CampusEats.Api.Features.MenuItem.DTOs;

public class CreateMenuItemRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, 999.99, ErrorMessage = "Price must be between 0.01 and 999.99")]
    public decimal Price { get; set; }

    public ICollection<int> AllergenIds { get; set; } = new List<int>();
}