using CampusEats.Api.Features.Payment.Stripe;
using CampusEats.Api.Infrastructure;
using CampusEats.Test.Helpers;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using CampusEats.Api.Utils.PaymentUtil;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CampusEats.Test.Handlers.Payment;

public class CreatePaymentIntentHandlerTests
{
    [Fact]
    public async Task Given_InvalidPaymentProvider_When_HandleIsCalled_Then_BadRequestReturned()
    {
        //Arrange
        string invalidProvider = "InvalidProvider";
        int orderId = 1;
        CreatePaymentIntentRequest request = new CreatePaymentIntentRequest(invalidProvider, orderId);
        
        // Create factory with empty list (no providers registered)
        PaymentProviderFactory factory = new PaymentProviderFactory(new List<IPaymentService>());
        Mock<IMenuItemRepository> mockedMenuItemRepo = new Mock<IMenuItemRepository>();
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var config = new ConfigurationBuilder().Build();
        
        CreatePaymentIntentHandler handler = new CreatePaymentIntentHandler(
            factory,
            mockedMenuItemRepo.Object,
            mockedOrderRepo.Object,
            dbContext,
            config
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status400BadRequest, httpResult.StatusCode);
    }
    
    [Fact]
    public async Task Given_NonExistentOrder_When_HandleIsCalled_Then_NotFoundReturned()
    {
        //Arrange
        string provider = "Stripe";
        int nonExistentOrderId = 999;
        CreatePaymentIntentRequest request = new CreatePaymentIntentRequest(provider, nonExistentOrderId);
        
        Mock<IPaymentService> mockedPaymentService = new Mock<IPaymentService>();
        mockedPaymentService.Setup(p => p.Name).Returns(provider);
        
        PaymentProviderFactory factory = new PaymentProviderFactory(
            new List<IPaymentService> { mockedPaymentService.Object }
        );
        Mock<IMenuItemRepository> mockedMenuItemRepo = new Mock<IMenuItemRepository>();
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var config = new ConfigurationBuilder().Build();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(nonExistentOrderId))
            .ReturnsAsync((Api.Models.Order?)null);
        
        CreatePaymentIntentHandler handler = new CreatePaymentIntentHandler(
            factory,
            mockedMenuItemRepo.Object,
            mockedOrderRepo.Object,
            dbContext,
            config
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status404NotFound, httpResult.StatusCode);
    }
    
    [Fact]
    public async Task Given_OrderWithNonExistentMenuItem_When_HandleIsCalled_Then_NotFoundReturned()
    {
        //Arrange
        string provider = "Stripe";
        int orderId = 1;
        int nonExistentMenuItemId = 999;
        CreatePaymentIntentRequest request = new CreatePaymentIntentRequest(provider, orderId);
        
        var order = new Api.Models.Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            OrderItems = new List<Api.Models.OrderItem>
            {
                new Api.Models.OrderItem { MenuItemId = nonExistentMenuItemId, Quantity = 2 }
            }
        };
        
        Mock<IPaymentService> mockedPaymentService = new Mock<IPaymentService>();
        mockedPaymentService.Setup(p => p.Name).Returns(provider);
        
        PaymentProviderFactory factory = new PaymentProviderFactory(
            new List<IPaymentService> { mockedPaymentService.Object }
        );
        Mock<IMenuItemRepository> mockedMenuItemRepo = new Mock<IMenuItemRepository>();
        Mock<IOrderRepository> mockedOrderRepo = new Mock<IOrderRepository>();
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var config = new ConfigurationBuilder().Build();
        
        mockedOrderRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        mockedMenuItemRepo.Setup(r => r.GetByIdAsync(nonExistentMenuItemId))
            .ReturnsAsync((Api.Models.MenuItem?)null);
        
        CreatePaymentIntentHandler handler = new CreatePaymentIntentHandler(
            factory,
            mockedMenuItemRepo.Object,
            mockedOrderRepo.Object,
            dbContext,
            config
        );
        
        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);
        
        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status404NotFound, httpResult.StatusCode);
    }

    [Fact]
    public async Task Given_ValidOrderWithLoyaltyPoints_When_HandleIsCalled_Then_DiscountApplied()
    {
        //Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Loyalty:DollarsPerPoint", "0.01" }
            })
            .Build();

        var user = new Api.Models.User 
        { 
            Username = "buyer", 
            Email = "buyer@test.com", 
            HashedPassword = "hash", 
            Role = Role.Buyer 
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var loyaltyAccount = new Api.Models.LoyaltyAccount 
        { 
            UserId = user.Id, 
            PointsBalance = 500 
        };
        dbContext.LoyaltyAccounts.Add(loyaltyAccount);
        await dbContext.SaveChangesAsync();

        var menuItem = new Api.Models.MenuItem 
        { 
            Name = "Burger", 
            Price = 10.00m, 
            Category = MenuCategory.Lunch, 
            IsAvailable = true 
        };
        dbContext.MenuItems.Add(menuItem);
        await dbContext.SaveChangesAsync();

        var order = new Api.Models.Order 
        { 
            UserId = user.Id, 
            Status = OrderStatus.Pending, 
            TotalAmount = 0,
            OrderItems = new List<Api.Models.OrderItem>
            {
                new Api.Models.OrderItem { MenuItemId = menuItem.Id, Quantity = 1 }
            }
        };
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        Mock<IPaymentService> mockedPaymentService = new Mock<IPaymentService>();
        mockedPaymentService.Setup(p => p.Name).Returns("Stripe");
        mockedPaymentService.Setup(p => p.CreatePaymentIntentAsync(
            It.Is<decimal>(amount => amount == 7.00m), // 10 - 3 (300 points * 0.01)
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<Guid>()))
            .ReturnsAsync(new Dictionary<string, string>
            {
                { "paymentIntentClientResult", "client_secret_123" },
                { "paymentIntentId", "pi_123" }
            });

        PaymentProviderFactory factory = new PaymentProviderFactory(
            new List<IPaymentService> { mockedPaymentService.Object }
        );
        
        var menuItemRepo = new MenuItemRepository(dbContext);
        var orderRepo = new OrderRepository(dbContext);

        CreatePaymentIntentHandler handler = new CreatePaymentIntentHandler(
            factory,
            menuItemRepo,
            orderRepo,
            dbContext,
            config
        );
        
        CreatePaymentIntentRequest request = new CreatePaymentIntentRequest("Stripe", order.Id, 300);

        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);

        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status200OK, httpResult.StatusCode);
        
        var updatedAccount = await dbContext.LoyaltyAccounts.FindAsync(loyaltyAccount.Id);
        Assert.NotNull(updatedAccount);
        Assert.Equal(200, updatedAccount.PointsBalance); // 500 - 300
        
        mockedPaymentService.Verify(p => p.CreatePaymentIntentAsync(7.00m, "usd", order.Id, user.Id), Times.Once);
    }

    [Fact]
    public async Task Given_InsufficientLoyaltyPoints_When_HandleIsCalled_Then_BadRequestReturned()
    {
        //Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var config = new ConfigurationBuilder().Build();

        var user = new Api.Models.User 
        { 
            Username = "buyer", 
            Email = "buyer@test.com", 
            HashedPassword = "hash", 
            Role = Role.Buyer 
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var loyaltyAccount = new Api.Models.LoyaltyAccount 
        { 
            UserId = user.Id, 
            PointsBalance = 100 
        };
        dbContext.LoyaltyAccounts.Add(loyaltyAccount);
        await dbContext.SaveChangesAsync();

        var menuItem = new Api.Models.MenuItem 
        { 
            Name = "Pizza", 
            Price = 10.00m, 
            Category = MenuCategory.Lunch, 
            IsAvailable = true 
        };
        dbContext.MenuItems.Add(menuItem);
        await dbContext.SaveChangesAsync();

        var order = new Api.Models.Order 
        { 
            UserId = user.Id, 
            Status = OrderStatus.Pending, 
            TotalAmount = 0,
            OrderItems = new List<Api.Models.OrderItem>
            {
                new Api.Models.OrderItem { MenuItemId = menuItem.Id, Quantity = 1 }
            }
        };
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        Mock<IPaymentService> mockedPaymentService = new Mock<IPaymentService>();
        mockedPaymentService.Setup(p => p.Name).Returns("Stripe");

        PaymentProviderFactory factory = new PaymentProviderFactory(
            new List<IPaymentService> { mockedPaymentService.Object }
        );
        
        var menuItemRepo = new MenuItemRepository(dbContext);
        var orderRepo = new OrderRepository(dbContext);

        CreatePaymentIntentHandler handler = new CreatePaymentIntentHandler(
            factory,
            menuItemRepo,
            orderRepo,
            dbContext,
            config
        );
        
        CreatePaymentIntentRequest request = new CreatePaymentIntentRequest("Stripe", order.Id, 200);

        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);

        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status400BadRequest, httpResult.StatusCode);
    }

    [Fact]
    public async Task Given_LoyaltyPointsWithoutAccount_When_HandleIsCalled_Then_BadRequestReturned()
    {
        //Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var config = new ConfigurationBuilder().Build();

        var user = new Api.Models.User 
        { 
            Username = "buyer", 
            Email = "buyer@test.com", 
            HashedPassword = "hash", 
            Role = Role.Buyer 
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var menuItem = new Api.Models.MenuItem 
        { 
            Name = "Pizza", 
            Price = 10.00m, 
            Category = MenuCategory.Lunch, 
            IsAvailable = true 
        };
        dbContext.MenuItems.Add(menuItem);
        await dbContext.SaveChangesAsync();

        var order = new Api.Models.Order 
        { 
            UserId = user.Id, 
            Status = OrderStatus.Pending, 
            TotalAmount = 0,
            OrderItems = new List<Api.Models.OrderItem>
            {
                new Api.Models.OrderItem { MenuItemId = menuItem.Id, Quantity = 1 }
            }
        };
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        Mock<IPaymentService> mockedPaymentService = new Mock<IPaymentService>();
        mockedPaymentService.Setup(p => p.Name).Returns("Stripe");

        PaymentProviderFactory factory = new PaymentProviderFactory(
            new List<IPaymentService> { mockedPaymentService.Object }
        );
        
        var menuItemRepo = new MenuItemRepository(dbContext);
        var orderRepo = new OrderRepository(dbContext);

        CreatePaymentIntentHandler handler = new CreatePaymentIntentHandler(
            factory,
            menuItemRepo,
            orderRepo,
            dbContext,
            config
        );
        
        CreatePaymentIntentRequest request = new CreatePaymentIntentRequest("Stripe", order.Id, 100);

        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);

        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status400BadRequest, httpResult.StatusCode);
    }

    [Fact]
    public async Task Given_OrderWithMultipleItems_When_HandleIsCalled_Then_TotalAmountCalculated()
    {
        //Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var config = new ConfigurationBuilder().Build();

        var user = new Api.Models.User 
        { 
            Username = "buyer", 
            Email = "buyer@test.com", 
            HashedPassword = "hash", 
            Role = Role.Buyer 
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var menuItem1 = new Api.Models.MenuItem 
        { 
            Name = "Pizza", 
            Price = 15.00m, 
            Category = MenuCategory.Lunch, 
            IsAvailable = true 
        };
        var menuItem2 = new Api.Models.MenuItem 
        { 
            Name = "Salad", 
            Price = 8.50m, 
            Category = MenuCategory.Breakfast, 
            IsAvailable = true 
        };
        dbContext.MenuItems.AddRange(menuItem1, menuItem2);
        await dbContext.SaveChangesAsync();

        var order = new Api.Models.Order 
        { 
            UserId = user.Id, 
            Status = OrderStatus.Pending, 
            TotalAmount = 0,
            OrderItems = new List<Api.Models.OrderItem>
            {
                new Api.Models.OrderItem { MenuItemId = menuItem1.Id, Quantity = 2 },
                new Api.Models.OrderItem { MenuItemId = menuItem2.Id, Quantity = 3 }
            }
        };
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        Mock<IPaymentService> mockedPaymentService = new Mock<IPaymentService>();
        mockedPaymentService.Setup(p => p.Name).Returns("Stripe");
        mockedPaymentService.Setup(p => p.CreatePaymentIntentAsync(
            It.Is<decimal>(amount => amount == 55.50m), // (15 * 2) + (8.50 * 3)
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<Guid>()))
            .ReturnsAsync(new Dictionary<string, string>
            {
                { "paymentIntentClientResult", "client_secret_123" },
                { "paymentIntentId", "pi_123" }
            });

        PaymentProviderFactory factory = new PaymentProviderFactory(
            new List<IPaymentService> { mockedPaymentService.Object }
        );
        
        var menuItemRepo = new MenuItemRepository(dbContext);
        var orderRepo = new OrderRepository(dbContext);

        CreatePaymentIntentHandler handler = new CreatePaymentIntentHandler(
            factory,
            menuItemRepo,
            orderRepo,
            dbContext,
            config
        );
        
        CreatePaymentIntentRequest request = new CreatePaymentIntentRequest("Stripe", order.Id);

        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);

        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status200OK, httpResult.StatusCode);
        
        mockedPaymentService.Verify(p => p.CreatePaymentIntentAsync(55.50m, "usd", order.Id, user.Id), Times.Once);
    }

    [Fact]
    public async Task Given_LoyaltyDiscountExceedingOrderAmount_When_HandleIsCalled_Then_DiscountCapped()
    {
        //Arrange
        var dbContext = DbContextHelper.CreateInMemoryDbContext();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Loyalty:DollarsPerPoint", "0.01" }
            })
            .Build();

        var user = new Api.Models.User 
        { 
            Username = "buyer", 
            Email = "buyer@test.com", 
            HashedPassword = "hash", 
            Role = Role.Buyer 
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var loyaltyAccount = new Api.Models.LoyaltyAccount 
        { 
            UserId = user.Id, 
            PointsBalance = 5000 
        };
        dbContext.LoyaltyAccounts.Add(loyaltyAccount);
        await dbContext.SaveChangesAsync();

        var menuItem = new Api.Models.MenuItem 
        { 
            Name = "Snack", 
            Price = 5.00m, 
            Category = MenuCategory.Desserts, 
            IsAvailable = true 
        };
        dbContext.MenuItems.Add(menuItem);
        await dbContext.SaveChangesAsync();

        var order = new Api.Models.Order 
        { 
            UserId = user.Id, 
            Status = OrderStatus.Pending, 
            TotalAmount = 0,
            OrderItems = new List<Api.Models.OrderItem>
            {
                new Api.Models.OrderItem { MenuItemId = menuItem.Id, Quantity = 1 }
            }
        };
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        Mock<IPaymentService> mockedPaymentService = new Mock<IPaymentService>();
        mockedPaymentService.Setup(p => p.Name).Returns("Stripe");
        mockedPaymentService.Setup(p => p.CreatePaymentIntentAsync(
            It.Is<decimal>(amount => amount == 0m),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<Guid>()))
            .ReturnsAsync(new Dictionary<string, string>
            {
                { "paymentIntentClientResult", "client_secret_123" },
                { "paymentIntentId", "pi_123" }
            });

        PaymentProviderFactory factory = new PaymentProviderFactory(
            new List<IPaymentService> { mockedPaymentService.Object }
        );
        
        var menuItemRepo = new MenuItemRepository(dbContext);
        var orderRepo = new OrderRepository(dbContext);

        CreatePaymentIntentHandler handler = new CreatePaymentIntentHandler(
            factory,
            menuItemRepo,
            orderRepo,
            dbContext,
            config
        );
        
        CreatePaymentIntentRequest request = new CreatePaymentIntentRequest("Stripe", order.Id, 1000);

        //Act
        IResult result = await handler.Handle(request, CancellationToken.None);

        //Assert
        var httpResult = result as IStatusCodeHttpResult;
        Assert.NotNull(httpResult);
        Assert.Equal(StatusCodes.Status200OK, httpResult.StatusCode);
        
        mockedPaymentService.Verify(p => p.CreatePaymentIntentAsync(0m, "usd", order.Id, user.Id), Times.Once);
    }
}
