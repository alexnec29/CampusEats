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
        
        app.MapGet("/test-loyalty", async (
            [FromServices] CampusEatsDbContext db,
            [FromServices] ILoyaltyAccountRepository accRepo,
            [FromServices] ILoyaltyTransactionRepository txRepo) =>
        {
            try
            {
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = $"testuser_{Guid.NewGuid():N}".Substring(0, 12),
                    Email = $"test_{Guid.NewGuid():N}@example.com",
                    HashedPassword = "hashedpassword",
                    Role = Role.Admin
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();

                var account = new LoyaltyAccount { UserId = user.Id, PointsBalance = 0 };
                await accRepo.AddAsync(account);

                var transaction = new LoyaltyTransaction
                {
                    LoyaltyAccountId = account.Id,
                    Points = 50,
                    TransactionType = "Earned",
                    Description = "Test Points"
                };
                await txRepo.AddAsync(transaction);
                
                account.PointsBalance = 100;
                await accRepo.UpdateAsync(account);

                transaction.Points = 75;
                transaction.Description = "Updated Points";
                await txRepo.UpdateAsync(transaction);
                
                var fetchedAccount = await accRepo.GetByIdAsync(account.Id);
                var fetchedTransaction = await txRepo.GetByIdAsync(transaction.Id);
                
                await txRepo.DeleteAsync(transaction.Id);
                await accRepo.DeleteAsync(account.Id);
                db.Users.Remove(user);
                await db.SaveChangesAsync();

                return Results.Ok(new
                {
                    Message = "✅ Loyalty full CRUD test completed",
                    UserId = user.Id,
                    AccountId = account.Id,
                    TransactionId = transaction.Id,
                    UpdatedPointsBalance = fetchedAccount?.PointsBalance,
                    UpdatedTransactionPoints = fetchedTransaction?.Points,
                    UpdatedTransactionDescription = fetchedTransaction?.Description
                });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return Results.Problem($"Exception: {ex.Message}, Inner: {inner}");
            }
        });
        
        app.MapGet("/test-kitchen", async ([FromServices] IKitchenTaskRepository repo,
            [FromServices] IOrderRepository orderRepo,
            [FromServices] CampusEatsDbContext db) =>
        {
            try
            {
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "kitchenstaff",
                    Email = "kitchen@example.com",
                    HashedPassword = "hashedpassword",
                    Role = Role.Kitchen
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();
                
                var order = new Order { UserId = user.Id, TotalAmount = 20m };
                await orderRepo.AddAsync(order);
                
                var task = new KitchenTask
                {
                    OrderId = order.Id,
                    Status = OrderStatus.Pending,
                    AssignedStaffId = user.Id
                };
                await repo.AddAsync(task);
                
                task.Status = OrderStatus.Completed;
                await repo.UpdateAsync(task);
                
                var tasks = await repo.GetAllAsync();
                
                await repo.DeleteAsync(task.Id);
                await orderRepo.DeleteAsync(order.Id);
                db.Users.Remove(user);
                await db.SaveChangesAsync();

                return Results.Ok(new
                {
                    Message = "✅ KitchenTask repository test completed",
                    TotalTasks = tasks.Count,
                    StaffId = user.Id,
                    OrderId = order.Id,
                    TaskId = task.Id
                });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return Results.Problem($"Exception: {ex.Message}, Inner: {inner}");
            }
        });

        app.MapGet("/test-order", async ([FromServices] CampusEatsDbContext db) =>
        {
            try
            {
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = $"orderuser_{Guid.NewGuid():N}".Substring(0, 12),
                    Email = $"order_{Guid.NewGuid():N}@example.com",
                    HashedPassword = "hashedpassword",
                    Role = Role.Buyer
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();

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
                    UserId = user.Id,
                    TotalAmount = menuItem.Price,
                    OrderItems = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            MenuItemId = menuItem.Id,
                            Quantity = 1,
                            Price = menuItem.Price
                        }
                    }
                };
                db.Orders.Add(order);
                await db.SaveChangesAsync();
                
                order.TotalAmount = 10m;
                order.Notes = "Updated order note";
                db.Orders.Update(order);
                await db.SaveChangesAsync();

                var orderItem = order.OrderItems.First();
                orderItem.Quantity = 2;
                orderItem.Price = 10m;
                db.OrderItems.Update(orderItem);
                await db.SaveChangesAsync();
                
                var fetchedOrder = await db.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == order.Id);
                
                db.OrderItems.Remove(orderItem);
                db.Orders.Remove(order);
                db.MenuItems.Remove(menuItem);
                db.Users.Remove(user);
                await db.SaveChangesAsync();

                return Results.Ok(new
                {
                    Message = "✅ Order full CRUD test completed",
                    UserId = user.Id,
                    OrderId = order.Id,
                    OrderItemId = orderItem.Id,
                    MenuItemId = menuItem.Id,
                    UpdatedTotalAmount = fetchedOrder?.TotalAmount,
                    UpdatedNotes = fetchedOrder?.Notes,
                    UpdatedOrderItemQuantity = fetchedOrder?.OrderItems.First().Quantity
                });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return Results.Problem($"Exception: {ex.Message}, Inner: {inner}");
            }
        });

    }
}
