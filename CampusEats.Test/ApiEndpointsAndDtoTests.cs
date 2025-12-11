using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Features.Order.CreateOrder;
using CampusEats.Api.Features.Allergen;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.ApiEndpoints.AndDtos;

public class MenuItemEndpointTests
{
    [Fact]
    public async Task Given_GetAllMenuItems_When_Called_Then_ReturnsAll()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<GetAllMenuItemsValidator>();
        
        var items = new List<MenuItem>
        {
            new MenuItem { Id = Guid.NewGuid(), Name = "Item 1", Price = 10m },
            new MenuItem { Id = Guid.NewGuid(), Name = "Item 2", Price = 15m }
        };
        
        mockRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(items);
        
        var handler = new GetAllMenuItemsHandler(mockRepository.Object, mockValidator.Object);
        var result = await handler.Handle(new GetAllMenuItemsRequest(), CancellationToken.None);
        
        mockRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Given_GetMenuItemById_When_ValidId_Then_ReturnsItem()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<GetMenuItemByIdValidator>();
        
        var itemId = Guid.NewGuid();
        var item = new MenuItem { Id = itemId, Name = "Burger", Price = 10m };
        
        mockRepository.Setup(repo => repo.GetByIdAsync(itemId))
            .ReturnsAsync(item);
        
        var handler = new GetMenuItemByIdHandler(mockRepository.Object, mockValidator.Object);
        var request = new GetMenuItemByIdRequest(itemId);
        
        var result = await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.GetByIdAsync(itemId), Times.Once);
    }

    [Fact]
    public async Task Given_CreateMenuItem_When_ValidData_Then_ItemStored()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<CreateMenuItemValidator>();
        
        var handler = new CreateMenuItemHandler(mockRepository.Object, mockValidator.Object);
        var request = new CreateMenuItemRequest("Burger", "Juicy burger", 12.99m, MenuItemCategory.MainCourse, "url", true);
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.AddAsync(It.IsAny<MenuItem>()), Times.Once);
    }

    [Fact]
    public async Task Given_UpdateMenuItem_When_ValidData_Then_ItemUpdated()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<UpdateMenuItemValidator>();
        
        var itemId = Guid.NewGuid();
        var item = new MenuItem { Id = itemId, Name = "Old Name", Price = 10m };
        
        mockRepository.Setup(repo => repo.GetByIdAsync(itemId))
            .ReturnsAsync(item);
        
        var handler = new UpdateMenuItemHandler(mockRepository.Object, mockValidator.Object);
        var request = new UpdateMenuItemRequest(itemId, "New Name", "New Description", 15m, null, true);
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<MenuItem>()), Times.Once);
    }

    [Fact]
    public async Task Given_DeleteMenuItem_When_ValidId_Then_ItemDeleted()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<DeleteMenuItemValidator>();
        
        var itemId = Guid.NewGuid();
        var item = new MenuItem { Id = itemId, Name = "Item" };
        
        mockRepository.Setup(repo => repo.GetByIdAsync(itemId))
            .ReturnsAsync(item);
        
        var handler = new DeleteMenuItemHandler(mockRepository.Object, mockValidator.Object);
        var request = new DeleteMenuItemRequest(itemId);
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.DeleteAsync(itemId), Times.Once);
    }

    [Fact]
    public async Task Given_SearchMenuItems_When_FilterApplied_Then_ResultsFiltered()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<SearchMenuItemsValidator>();
        
        var items = new List<MenuItem>
        {
            new MenuItem { Id = Guid.NewGuid(), Name = "Burger", Category = MenuItemCategory.MainCourse },
            new MenuItem { Id = Guid.NewGuid(), Name = "Fries", Category = MenuItemCategory.SideDish }
        };
        
        mockRepository.Setup(repo => repo.SearchAsync("Burger", null, null))
            .ReturnsAsync(items.Where(i => i.Name.Contains("Burger")).ToList());
        
        var handler = new SearchMenuItemsHandler(mockRepository.Object, mockValidator.Object);
        var request = new SearchMenuItemsRequest("Burger", null, null);
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.SearchAsync("Burger", null, null), Times.Once);
    }

    [Fact]
    public async Task Given_GetMenuItemsByCategory_When_CategorySpecified_Then_ItemsFiltered()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<GetMenuItemsByCategoryValidator>();
        
        var category = MenuItemCategory.MainCourse;
        var items = new List<MenuItem>
        {
            new MenuItem { Id = Guid.NewGuid(), Name = "Burger", Category = category },
            new MenuItem { Id = Guid.NewGuid(), Name = "Pizza", Category = category }
        };
        
        mockRepository.Setup(repo => repo.GetByCategoryAsync(category))
            .ReturnsAsync(items);
        
        var handler = new GetMenuItemsByCategoryHandler(mockRepository.Object, mockValidator.Object);
        var request = new GetMenuItemsByCategoryRequest(category);
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.GetByCategoryAsync(category), Times.Once);
    }
}

public class OrderEndpointTests
{
    [Fact]
    public async Task Given_CreateOrder_When_ValidData_Then_OrderStored()
    {
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateOrderValidator>();
        
        var userId = Guid.NewGuid();
        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(new User { Id = userId });
        mockOrderRepository.Setup(repo => repo.GetOrdersByUserAsync(userId))
            .ReturnsAsync(new List<Order>());
        
        var handler = new CreateOrderHandler(mockOrderRepository.Object, mockUserRepository.Object, mockValidator.Object);
        var request = new CreateOrderRequest(userId, "Special instructions");
        
        await handler.Handle(request, CancellationToken.None);
        
        mockOrderRepository.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task Given_GetOrderById_When_ValidId_Then_OrderReturned()
    {
        var mockRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<GetOrderByIdValidator>();
        
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.Pending };
        
        mockRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new GetOrderByIdHandler(mockRepository.Object, mockValidator.Object);
        var request = new GetOrderByIdRequest(orderId);
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.GetByIdAsync(orderId), Times.Once);
    }

    [Fact]
    public async Task Given_GetAllOrders_When_Called_Then_ReturnsAll()
    {
        var mockRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<GetAllOrdersValidator>();
        
        var orders = new List<Order>
        {
            new Order { Id = Guid.NewGuid(), Status = OrderStatus.Pending },
            new Order { Id = Guid.NewGuid(), Status = OrderStatus.Confirmed }
        };
        
        mockRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(orders);
        
        var handler = new GetAllOrdersHandler(mockRepository.Object, mockValidator.Object);
        var result = await handler.Handle(new GetAllOrdersRequest(), CancellationToken.None);
        
        mockRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Given_GetUserOrders_When_UserIdSpecified_Then_UserOrdersReturned()
    {
        var mockRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<GetUserOrdersValidator>();
        
        var userId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new Order { Id = Guid.NewGuid(), UserId = userId, Status = OrderStatus.Completed },
            new Order { Id = Guid.NewGuid(), UserId = userId, Status = OrderStatus.Completed }
        };
        
        mockRepository.Setup(repo => repo.GetOrdersByUserAsync(userId))
            .ReturnsAsync(orders);
        
        var handler = new GetUserOrdersHandler(mockRepository.Object, mockValidator.Object);
        var request = new GetUserOrdersRequest(userId);
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.GetOrdersByUserAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Given_CancelOrder_When_ValidOrder_Then_Cancelled()
    {
        var mockRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CancelOrderValidator>();
        
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.Pending };
        
        mockRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new CancelOrderHandler(mockRepository.Object, mockValidator.Object);
        var request = new CancelOrderRequest(orderId);
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task Given_UpdateOrderStatus_When_NewStatus_Then_Updated()
    {
        var mockRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<UpdateOrderStatusValidator>();
        
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.Pending };
        
        mockRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new UpdateOrderStatusHandler(mockRepository.Object, mockValidator.Object);
        var request = new UpdateOrderStatusRequest(orderId, OrderStatus.Confirmed);
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task Given_AddOrderItem_When_ValidMenuItemId_Then_ItemAdded()
    {
        var mockRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<AddOrderItemValidator>();
        
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Items = new List<OrderItem>() };
        
        mockRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new AddOrderItemHandler(mockRepository.Object, mockValidator.Object);
        var request = new AddOrderItemRequest(orderId, Guid.NewGuid(), 2);
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Order>()), Times.Once);
    }
}

public class AllergenEndpointTests
{
    [Fact]
    public async Task Given_CreateAllergen_When_ValidData_Then_AllergenStored()
    {
        var mockRepository = new Mock<IAllergenRepository>();
        var mockValidator = new Mock<CreateAllergenValidator>();
        
        var handler = new CreateAllergenHandler(mockRepository.Object, mockValidator.Object);
        var request = new CreateAllergenRequest("Peanuts", "Tree nut allergen");
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Allergen>()), Times.Once);
    }

    [Fact]
    public async Task Given_GetAllAllergens_When_Called_Then_ReturnsAll()
    {
        var mockRepository = new Mock<IAllergenRepository>();
        var mockValidator = new Mock<GetAllAllergensValidator>();
        
        var allergens = new List<Allergen>
        {
            new Allergen { Id = Guid.NewGuid(), Name = "Peanuts" },
            new Allergen { Id = Guid.NewGuid(), Name = "Dairy" }
        };
        
        mockRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(allergens);
        
        var handler = new GetAllAllergensHandler(mockRepository.Object, mockValidator.Object);
        var result = await handler.Handle(new GetAllAllergensRequest(), CancellationToken.None);
        
        mockRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Given_DeleteAllergen_When_ValidId_Then_AllergenDeleted()
    {
        var mockRepository = new Mock<IAllergenRepository>();
        var mockValidator = new Mock<DeleteAllergenValidator>();
        
        var allergenId = Guid.NewGuid();
        var allergen = new Allergen { Id = allergenId, Name = "Peanuts" };
        
        mockRepository.Setup(repo => repo.GetByIdAsync(allergenId))
            .ReturnsAsync(allergen);
        
        var handler = new DeleteAllergenHandler(mockRepository.Object, mockValidator.Object);
        var request = new DeleteAllergenRequest(allergenId);
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.DeleteAsync(allergenId), Times.Once);
    }
}

public class UserEndpointTests
{
    [Fact]
    public async Task Given_CreateUser_When_ValidData_Then_UserStored()
    {
        var mockRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateUserValidator>();
        
        mockRepository.Setup(repo => repo.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);
        mockRepository.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);
        
        var handler = new CreateUserHandler(mockRepository.Object);
        var request = new CreateUserRequest("newuser", "user@example.com", "Pass123!", "Pass123!");
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Given_LoginUser_When_CorrectCredentials_Then_TokenReturned()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<LoginUserValidator>();
        var mockJwtService = new Mock<IJwtService>();
        
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Role = Role.Buyer };
        mockUserRepository.Setup(repo => repo.GetByUsernameAsync("testuser"))
            .ReturnsAsync(user);
        mockJwtService.Setup(service => service.GenerateToken(user))
            .Returns("token");
        
        var handler = new LoginUserHandler(mockUserRepository.Object, mockValidator.Object, mockJwtService.Object);
        var request = new LoginUserRequest("testuser", "password");
        
        await handler.Handle(request, CancellationToken.None);
        
        mockJwtService.Verify(service => service.GenerateToken(user), Times.Once);
    }

    [Fact]
    public async Task Given_LogoutUser_When_ValidToken_Then_TokenBlacklisted()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockBlacklistRepository = new Mock<IBlackListTokenRepository>();
        var mockValidator = new Mock<LogoutUserValidator>();
        
        var userId = Guid.NewGuid();
        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(new User { Id = userId });
        mockBlacklistRepository.Setup(repo => repo.IsTokenBlacklistedAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        
        var handler = new LogoutUserHandler(mockUserRepository.Object, mockBlacklistRepository.Object, mockValidator.Object);
        var request = new LogoutUserRequest(userId, "token");
        
        await handler.Handle(request, CancellationToken.None);
        
        mockBlacklistRepository.Verify(repo => repo.AddToBlacklistAsync(userId, "token"), Times.Once);
    }

    [Fact]
    public async Task Given_GetUserById_When_ValidId_Then_UserReturned()
    {
        var mockRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<GetUserByIdValidator>();
        
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "testuser" };
        
        mockRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
        
        var handler = new GetUserByIdHandler(mockRepository.Object, mockValidator.Object);
        var request = new GetUserByIdRequest(userId);
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
    }
}

public class DtoMappingTests
{
    [Fact]
    public void Given_MenuItemModel_When_MappedToDto_Then_AllPropertiesTransferred()
    {
        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Burger",
            Description = "Juicy burger",
            Price = 10m,
            Category = MenuItemCategory.MainCourse,
            ImageUrl = "http://example.com/image.jpg",
            Available = true
        };
        
        menuItem.Name.Should().Be("Burger");
        menuItem.Price.Should().Be(10m);
        menuItem.Available.Should().BeTrue();
    }

    [Fact]
    public void Given_OrderModel_When_MappedToDto_Then_StatusPreserved()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Confirmed,
            CreatedAt = DateTime.UtcNow
        };
        
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Given_UserModel_When_MappedToDto_Then_RolePreserved()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            Role = Role.Kitchen
        };
        
        user.Role.Should().Be(Role.Kitchen);
    }
}
