using Microsoft.EntityFrameworkCore;
using CampusEats.Api.Models;

namespace CampusEats.Api.Infrastructure;

public static class DbInitializer
{
    public static async Task InitializeAsync(CampusEatsDbContext context)
    {
        // Aplică toate migrations
        await context.Database.MigrateAsync();

        // --- Seed Allergens (exemplu) ---
        if (!context.Allergens.Any())
        {
            context.Allergens.AddRange(
                new Allergen { Name = "Peanuts" },
                new Allergen { Name = "Gluten" },
                new Allergen { Name = "Dairy" }
            );
            await context.SaveChangesAsync();
        }
        
        if (!context.MenuItems.Any())
        {
            context.MenuItems.AddRange(
                new MenuItem { Name = "Pizza", Price = 10m },
                new MenuItem { Name = "Burger", Price = 8m },
                new MenuItem { Name = "Salad", Price = 6m },
                new MenuItem { Name = "Soup", Price = 5m },
                new MenuItem { Name = "Pasta", Price = 9m }
            );
            await context.SaveChangesAsync();
        }
        
        //TODO: implement when User class and UserRole exist
    }
}