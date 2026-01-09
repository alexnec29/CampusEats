using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Infrastructure;

public static class DbInitializer
{
    public static async Task InitializeAsync(CampusEatsDbContext context)
    {
        // Aplică toate migrations
        // await context.Database.MigrateAsync();

        // --- Seed Allergens (exemplu) ---
        if (!await context.Allergens.AnyAsync())
        {
            context.Allergens.AddRange(
                new Allergen { Name = "Peanuts" },
                new Allergen { Name = "Gluten" },
                new Allergen { Name = "Dairy" }
            );
            await context.SaveChangesAsync();
        }
        
        if (!await context.MenuItems.AnyAsync())
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

        if (!await context.Users.AnyAsync())
        {
            context.Users.AddRange(
                new User { Username = "buyer1", HashedPassword = BCrypt.Net.BCrypt.HashPassword("Buyer1$4"), Email = "buyer1@gmail.com", Role = Role.Buyer },
                new User { Username = "buyer2", HashedPassword = BCrypt.Net.BCrypt.HashPassword("Buyer2$4"), Email = "buyer2@gmail.com", Role = Role.Buyer },
                new User { Username = "kitchen", HashedPassword = BCrypt.Net.BCrypt.HashPassword("kitcheN$4"), Email = "kitchen@gmail.com", Role = Role.Kitchen },
                new User { Username = "admin", HashedPassword = BCrypt.Net.BCrypt.HashPassword("admiN$$4"), Email = "admin@gmail.com", Role = Role.Admin }
            );
            await context.SaveChangesAsync();
        }
    }
}