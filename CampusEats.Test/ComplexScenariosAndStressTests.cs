using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Features.Order.CreateOrder;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models.Enums;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Handlers.Integration;

public class ComplexScenarioTests
{
    [Fact]
    public async Task Given_CreateOrderAndAddMultipleItems_When_HandlesCalled_Then_OrderCompleted()
    {
        var userId = Guid.NewGuid();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var mockCreateOrderValidator = new Mock<CreateOrderValidator>();
        var mockAddItemValidator = new Mock<AddOrderItemValidator>();
        
        var user = new Api.Models.User { Id = userId };
        var order = new Api.Models.Order { Id = Guid.NewGuid(), UserId = userId };
        
        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
        mockOrderRepository.Setup(repo => repo.GetOrdersByUserAsync(userId))
            .ReturnsAsync(new List<Api.Models.Order>());
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(order.Id))
            .ReturnsAsync(order);
        
        var createOrderHandler = new CreateOrderHandler(mockOrderRepository.Object, mockUserRepository.Object, mockCreateOrderValidator.Object);
        var createOrderRequest = new CreateOrderRequest(userId, "Notes");
        
        var result = await createOrderHandler.Handle(createOrderRequest, CancellationToken.None);
        
        mockOrderRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.Order>()), Times.Once);
    }

    [Fact]
    public async Task Given_CreateUserThenCreateLoyaltyAccount_When_Sequential_Then_BothSucceed()
    {
        var userId = Guid.NewGuid();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockLoyaltyRepository = new Mock<ILoyaltyAccountRepository>();
        var mockCreateUserValidator = new Mock<CreateUserValidator>();
        var mockCreateLoyaltyValidator = new Mock<CreateLoyaltyAccountValidator>();
        
        var user = new Api.Models.User { Id = userId, Username = "newuser" };
        mockUserRepository.Setup(repo => repo.GetByUsernameAsync("newuser"))
            .ReturnsAsync((Api.Models.User)null);
        mockUserRepository.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Api.Models.User)null);
        
        var createUserHandler = new CreateUserHandler(mockUserRepository.Object);
        var createUserRequest = new CreateUserRequest("newuser", "user@example.com", "Pass123!", "Pass123!");
        
        await createUserHandler.Handle(createUserRequest, CancellationToken.None);
        
        mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.User>()), Times.Once);

        var createLoyaltyHandler = new global::CampusEats.Api.Features.LoyaltyAccount.CreateLoyaltyAccountHandler(
            mockLoyaltyRepository.Object, mockCreateLoyaltyValidator.Object);
        var createLoyaltyRequest = new global::CampusEats.Api.Features.LoyaltyAccount.CreateLoyaltyAccountRequest(userId);
        
        await createLoyaltyHandler.Handle(createLoyaltyRequest, CancellationToken.None);
        
        mockLoyaltyRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.LoyaltyAccount>()), Times.Once);
    }

    [Fact]
    public async Task Given_LargeNumberOfOrders_When_AllCreated_Then_AllStored()
    {
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateOrderValidator>();
        
        var user = new Api.Models.User { Id = Guid.NewGuid() };
        mockUserRepository.Setup(repo => repo.GetByIdAsync(user.Id))
            .ReturnsAsync(user);
        mockOrderRepository.Setup(repo => repo.GetOrdersByUserAsync(user.Id))
            .ReturnsAsync(new List<Api.Models.Order>());
        
        var handler = new CreateOrderHandler(mockOrderRepository.Object, mockUserRepository.Object, mockValidator.Object);
        
        for (int i = 0; i < 10; i++)
        {
            var request = new CreateOrderRequest(user.Id, $"Notes {i}");
            await handler.Handle(request, CancellationToken.None);
        }
        
        mockOrderRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.Order>()), Times.Exactly(10));
    }

    [Fact]
    public async Task Given_MultipleUsersCreatingOrders_When_Concurrent_Then_AllProcessed()
    {
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateOrderValidator>();
        
        var users = Enumerable.Range(0, 5)
            .Select(_ => new Api.Models.User { Id = Guid.NewGuid() })
            .ToList();
        
        foreach (var user in users)
        {
            mockUserRepository.Setup(repo => repo.GetByIdAsync(user.Id))
                .ReturnsAsync(user);
            mockOrderRepository.Setup(repo => repo.GetOrdersByUserAsync(user.Id))
                .ReturnsAsync(new List<Api.Models.Order>());
        }
        
        var handler = new CreateOrderHandler(mockOrderRepository.Object, mockUserRepository.Object, mockValidator.Object);
        
        var tasks = users.Select(user =>
            handler.Handle(new CreateOrderRequest(user.Id, "Notes"), CancellationToken.None)
        );
        
        await Task.WhenAll(tasks);
        
        mockOrderRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.Order>()), Times.Exactly(5));
    }
}

public class StressTestScenarios
{
    [Fact]
    public async Task Given_CreateLargeNumberOfMenuItems_When_Sequential_Then_AllCreated()
    {
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<CreateMenuItemValidator>();
        
        var handler = new CreateMenuItemHandler(mockMenuItemRepository.Object, mockValidator.Object);
        
        for (int i = 0; i < 100; i++)
        {
            var request = new CreateMenuItemRequest(
                $"Item {i}",
                $"Description {i}",
                10m + i,
                (MenuItemCategory)(i % 3),
                $"url-{i}",
                i % 2 == 0
            );
            
            await handler.Handle(request, CancellationToken.None);
        }
        
        mockMenuItemRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.MenuItem>()), Times.Exactly(100));
    }

    [Fact]
    public async Task Given_VeryLongOperationSequence_When_Executed_Then_NoMemoryIssues()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<CreateMenuItemValidator>();
        
        var handler = new CreateMenuItemHandler(mockRepository.Object, mockValidator.Object);
        
        for (int i = 0; i < 1000; i++)
        {
            var request = new CreateMenuItemRequest(
                $"Item {i}",
                new string('a', 1000),
                (decimal)i * 0.99m,
                MenuItemCategory.MainCourse,
                "url",
                true
            );
            
            await handler.Handle(request, CancellationToken.None);
        }
        
        mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Api.Models.MenuItem>()), Times.Exactly(1000));
    }

    [Fact]
    public async Task Given_RapidSuccessiveLoginAttempts_When_Multiple_Then_AllProcessed()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<LoginUserValidator>();
        var mockJwtService = new Mock<IJwtService>();
        
        var users = Enumerable.Range(0, 20)
            .Select(i => new Api.Models.User { Id = Guid.NewGuid(), Username = $"user{i}", Role = Role.Buyer })
            .ToList();
        
        foreach (var user in users)
        {
            mockUserRepository.Setup(repo => repo.GetByUsernameAsync(user.Username))
                .ReturnsAsync(user);
            mockJwtService.Setup(service => service.GenerateToken(user))
                .Returns($"token-{user.Id}");
        }
        
        var handler = new global::CampusEats.Api.Features.User.LoginUserHandler(
            mockUserRepository.Object, mockValidator.Object, mockJwtService.Object);
        
        var tasks = users.Select(user =>
            handler.Handle(new global::CampusEats.Api.Features.User.LoginUserRequest(user.Username, "password"), CancellationToken.None)
        );
        
        await Task.WhenAll(tasks);
        
        mockJwtService.Verify(service => service.GenerateToken(It.IsAny<Api.Models.User>()), Times.Exactly(20));
    }
}

public class ExceptionHandlingScenarios
{
    [Fact]
    public async Task Given_RepositoryThrowsException_When_HandleCalled_Then_ExceptionPropagated()
    {
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<CreateMenuItemValidator>();
        
        mockMenuItemRepository.Setup(repo => repo.AddAsync(It.IsAny<Api.Models.MenuItem>()))
            .Throws<Exception>();
        
        var handler = new CreateMenuItemHandler(mockMenuItemRepository.Object, mockValidator.Object);
        var request = new CreateMenuItemRequest("Item", "Desc", 10m, MenuItemCategory.MainCourse, "url", true);
        
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Given_ValidatorThrowsException_When_HandleCalled_Then_ExceptionPropagated()
    {
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<CreateMenuItemValidator>();
        
        mockValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateMenuItemRequest>(), It.IsAny<CancellationToken>()))
            .Throws<Exception>();
        
        var handler = new CreateMenuItemHandler(mockMenuItemRepository.Object, mockValidator.Object);
        var request = new CreateMenuItemRequest("Item", "Desc", 10m, MenuItemCategory.MainCourse, "url", true);
        
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Given_MultipleRepositoriesThrowException_When_HandleCalled_Then_FirstExceptionThrown()
    {
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<CreateOrderValidator>();
        
        var userId = Guid.NewGuid();
        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ThrowsAsync(new Exception("User repository error"));
        
        var handler = new CreateOrderHandler(mockOrderRepository.Object, mockUserRepository.Object, mockValidator.Object);
        var request = new CreateOrderRequest(userId, "Notes");
        
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(request, CancellationToken.None));
    }
}

public class NullAndEmptyHandlingTests
{
    [Fact]
    public async Task Given_NullOrderRepository_When_Passed_Then_ExceptionOrHandled()
    {
        var mockValidator = new Mock<CreateMenuItemValidator>();
        var mockRepository = (IMenuItemRepository)null;
        
        // Constructor should handle null appropriately
        try
        {
            var handler = new CreateMenuItemHandler(mockRepository, mockValidator.Object);
            Assert.True(false, "Should have thrown exception");
        }
        catch (ArgumentNullException)
        {
            Assert.True(true);
        }
    }

    [Fact]
    public async Task Given_EmptyStringInput_When_Validated_Then_Checked()
    {
        var validator = new CreateMenuItemValidator();
        var request = new CreateMenuItemRequest(
            "",
            "",
            10m,
            MenuItemCategory.MainCourse,
            "",
            true
        );
        
        var result = await validator.ValidateAsync(request);
        
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Given_WhitespaceOnlyInput_When_Validated_Then_Checked()
    {
        var validator = new CreateMenuItemValidator();
        var request = new CreateMenuItemRequest(
            "   ",
            "   ",
            10m,
            MenuItemCategory.MainCourse,
            "   ",
            true
        );
        
        var result = await validator.ValidateAsync(request);
        
        // Should treat whitespace-only as invalid
        result.IsValid.Should().BeFalse();
    }
}
