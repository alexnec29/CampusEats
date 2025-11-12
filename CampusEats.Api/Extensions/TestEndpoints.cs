using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Infrastructure;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace CampusEats.Api.Extensions;

public static class TestEndpoints
{
    public static void MapTestEndpoints(this WebApplication app)
    {
        app.MapGet("/db-test", async ([FromServices] CampusEatsDbContext db) =>
        {
            try
            {
                var allergensCount = await db.Allergens.CountAsync();
                var menuCount = await db.MenuItems.CountAsync();
                var ordersCount = await db.Orders.CountAsync();

                return Results.Ok(new
                {
                    Message = "✅ DB and seed working",
                    Allergens = allergensCount,
                    MenuItems = menuCount,
                    Orders = ordersCount
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"❌ Connection failed: {ex.Message}");
            }
        });
        
        app.MapGet("/test-menu", async ([FromServices] IMenuItemRepository repo) =>
        {
            var newItem = new MenuItem
            {
                Name = "Test Pizza",
                Price = 11.5m,
                Category = MenuCategory.Breakfast,
                IsAvailable = true
            };
            await repo.AddAsync(newItem);

            newItem.Price = 12.0m;
            await repo.UpdateAsync(newItem);

            var allItems = await repo.GetAllAsync();
            await repo.DeleteAsync(newItem.Id);

            return Results.Ok(new
            {
                Message = "Menu repository test completed",
                TotalMenuItems = allItems.Count
            });
        });
    }
}
