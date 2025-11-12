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

        app.MapGet("/test-allergen", async ([FromServices] IAllergenRepository repo) =>
        {
            var newAllergen = new Allergen
            {
                Name = "Test Allergen",
                Description = "Testing"
            };
            await repo.AddAsync(newAllergen);

            newAllergen.Description = "Updated description";
            await repo.UpdateAsync(newAllergen);

            var allAllergens = await repo.GetAllAsync();
            await repo.DeleteAsync(newAllergen.Id);

            return Results.Ok(new
            {
                Message = "Allergen repository test completed",
                TotalAllergens = allAllergens.Count
            });
        });

        app.MapGet("/test-order", async ([FromServices] CampusEatsDbContext db) =>
        {
            var menuItem = new MenuItem
            {
                Name = "Test Burger",
                Price = 5m,
                Category = MenuCategory.Lunch,
                IsAvailable = true
            };
            db.MenuItems.Add(menuItem);
            await db.SaveChangesAsync();

            var order = new Order
            {
                UserId = 1,
                TotalAmount = 5m,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        MenuItem = menuItem,
                        Quantity = 1,
                        Price = menuItem.Price
                    }
                }
            };
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var orders = await db.Orders
                .Include(o => o.OrderItems)
                .ToListAsync();

            db.Orders.Remove(order);
            db.MenuItems.Remove(menuItem);
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                Message = "Order repository test completed",
                TotalOrders = orders.Count
            });
        });

        app.MapGet("/test-payment", async ([FromServices] IPaymentRepository repo, [FromServices] IOrderRepository orderRepo) =>
        {
            var order = new Order { UserId = 1, TotalAmount = 10m };
            await orderRepo.AddAsync(order);

            var payment = new Payment { OrderId = order.Id, Amount = 10m, Method = PaymentMethod.Card };
            await repo.AddAsync(payment);

            payment.Status = PaymentStatus.Completed;
            await repo.UpdateAsync(payment);

            var payments = await repo.GetAllAsync();

            await repo.DeleteAsync(payment.Id);
            await orderRepo.DeleteAsync(order.Id);

            return Results.Ok(new
            {
                Message = "Payment repository test completed",
                TotalPayments = payments.Count
            });
        });

        app.MapGet("/test-loyalty", async ([FromServices] ILoyaltyAccountRepository accRepo, [FromServices] ILoyaltyTransactionRepository txRepo) =>
        {
            var account = new LoyaltyAccount { UserId = 1 };
            await accRepo.AddAsync(account);

            var transaction = new LoyaltyTransaction
            {
                LoyaltyAccountId = account.Id,
                Points = 50,
                TransactionType = "Earned",
                Description = "Test Points"
            };
            await txRepo.AddAsync(transaction);

            account.PointsBalance = 50;
            await accRepo.UpdateAsync(account);

            var accounts = await accRepo.GetAllAsync();
            await txRepo.DeleteAsync(transaction.Id);
            await accRepo.DeleteAsync(account.Id);

            return Results.Ok(new
            {
                Message = "Loyalty repository test completed",
                TotalAccounts = accounts.Count
            });
        });

        app.MapGet("/test-kitchen", async ([FromServices] IKitchenTaskRepository repo, [FromServices] IOrderRepository orderRepo) =>
        {
            var order = new Order { UserId = 1, TotalAmount = 20m };
            await orderRepo.AddAsync(order);

            var task = new KitchenTask { OrderId = order.Id, Status = OrderStatus.Pending };
            await repo.AddAsync(task);

            task.Status = OrderStatus.Completed;
            await repo.UpdateAsync(task);

            var tasks = await repo.GetAllAsync();

            await repo.DeleteAsync(task.Id);
            await orderRepo.DeleteAsync(order.Id);

            return Results.Ok(new
            {
                Message = "KitchenTask repository test completed",
                TotalTasks = tasks.Count
            });
        });
    }
}