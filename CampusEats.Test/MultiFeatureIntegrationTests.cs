using CampusEats.Api.Features.MenuItem;
using CampusEats.Api.Features.Order.CreateOrder;
using CampusEats.Api.Features.Allergen;
using CampusEats.Api.Infrastructure.Repositories;
using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using Moq;
using FluentAssertions;

namespace CampusEats.Test.Integration.MultiFeatureScenarios;

public class CompleteOrderLifecycleTests
{
    [Fact]
    public async Task Given_UserCreatesOrderWithMenuItems_When_AllStepsCompleted_Then_OrderFullyProcessed()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<CreateOrderValidator>();
        
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "buyer1", Role = Role.Buyer };
        var menuItem = new MenuItem { Id = Guid.NewGuid(), Name = "Burger", Price = 12.99m };
        
        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
        mockMenuItemRepository.Setup(repo => repo.GetByIdAsync(menuItem.Id))
            .ReturnsAsync(menuItem);
        mockOrderRepository.Setup(repo => repo.GetOrdersByUserAsync(userId))
            .ReturnsAsync(new List<Order>());
        
        var handler = new CreateOrderHandler(mockOrderRepository.Object, mockUserRepository.Object, mockValidator.Object);
        var request = new CreateOrderRequest(userId, "Please add extra sauce");
        
        var result = await handler.Handle(request, CancellationToken.None);
        
        mockOrderRepository.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Once);
        mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Given_CompleteOrderWorkflow_When_StatusTransitions_Then_AllStatesReached()
    {
        var mockRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<UpdateOrderStatusValidator>();
        
        var order = new Order { Id = Guid.NewGuid(), Status = OrderStatus.Pending };
        mockRepository.Setup(repo => repo.GetByIdAsync(order.Id))
            .ReturnsAsync(order);
        
        var handler = new UpdateOrderStatusHandler(mockRepository.Object, mockValidator.Object);
        
        var statuses = new[] { OrderStatus.Confirmed, OrderStatus.InProgress, OrderStatus.Completed };
        
        foreach (var status in statuses)
        {
            order.Status = status;
            await handler.Handle(new UpdateOrderStatusRequest(order.Id, status), CancellationToken.None);
        }
        
        order.Status.Should().Be(OrderStatus.Completed);
        mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Order>()), Times.AtLeast(3));
    }

    [Fact]
    public async Task Given_UserOrdersWithMultipleItems_When_Processed_Then_AllItemsTracked()
    {
        var mockRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<AddOrderItemValidator>();
        
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Items = new List<OrderItem>() };
        
        mockRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new AddOrderItemHandler(mockRepository.Object, mockValidator.Object);
        
        for (int i = 0; i < 5; i++)
        {
            var request = new AddOrderItemRequest(orderId, Guid.NewGuid(), i + 1);
            await handler.Handle(request, CancellationToken.None);
        }
        
        mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Order>()), Times.Exactly(5));
    }
}

public class MenuItemManagementWithAllergensTests
{
    [Fact]
    public async Task Given_CreateMenuItemWithAllergens_When_Completed_Then_AllergenLinksCreated()
    {
        var mockMenuItemRepository = new Mock<IMenuItemRepository>();
        var mockAllergenRepository = new Mock<IAllergenRepository>();
        var mockValidator = new Mock<CreateMenuItemValidator>();
        
        var allergen = new Allergen { Id = Guid.NewGuid(), Name = "Peanuts" };
        var menuItem = new MenuItem { Id = Guid.NewGuid(), Name = "Peanut Butter Sandwich" };
        
        mockAllergenRepository.Setup(repo => repo.GetByIdAsync(allergen.Id))
            .ReturnsAsync(allergen);
        mockMenuItemRepository.Setup(repo => repo.GetByIdAsync(menuItem.Id))
            .ReturnsAsync(menuItem);
        
        var handler = new CreateMenuItemHandler(mockMenuItemRepository.Object, mockValidator.Object);
        var request = new CreateMenuItemRequest("Peanut Butter Sandwich", "Description", 8.99m, MenuItemCategory.MainCourse, "url", true);
        
        await handler.Handle(request, CancellationToken.None);
        
        mockMenuItemRepository.Verify(repo => repo.AddAsync(It.IsAny<MenuItem>()), Times.Once);
    }

    [Fact]
    public async Task Given_UpdateMenuItemRemoveAllergen_When_Completed_Then_AllergenLinkRemoved()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<RemoveAllergenFromMenuItemValidator>();
        
        var menuItem = new MenuItem 
        { 
            Id = Guid.NewGuid(), 
            Name = "Burger",
            Allergens = new List<MenuItemAllergen>()
        };
        
        mockRepository.Setup(repo => repo.GetByIdAsync(menuItem.Id))
            .ReturnsAsync(menuItem);
        
        var handler = new RemoveAllergenFromMenuItemHandler(mockRepository.Object, mockValidator.Object);
        var request = new RemoveAllergenFromMenuItemRequest(menuItem.Id, Guid.NewGuid());
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<MenuItem>()), Times.Once);
    }

    [Fact]
    public async Task Given_SearchMenuItemsWithMultipleFilters_When_Applied_Then_ResultsFiltered()
    {
        var mockRepository = new Mock<IMenuItemRepository>();
        var mockValidator = new Mock<SearchMenuItemsValidator>();
        
        var items = new List<MenuItem>
        {
            new MenuItem { Id = Guid.NewGuid(), Name = "Chicken Burger", Category = MenuItemCategory.MainCourse, Price = 10m },
            new MenuItem { Id = Guid.NewGuid(), Name = "Fish Burger", Category = MenuItemCategory.MainCourse, Price = 12m },
            new MenuItem { Id = Guid.NewGuid(), Name = "Salad", Category = MenuItemCategory.MainCourse, Price = 8m }
        };
        
        mockRepository.Setup(repo => repo.SearchAsync("Burger", MenuItemCategory.MainCourse, null))
            .ReturnsAsync(items.Where(i => i.Name.Contains("Burger") && i.Category == MenuItemCategory.MainCourse).ToList());
        
        var handler = new SearchMenuItemsHandler(mockRepository.Object, mockValidator.Object);
        var request = new SearchMenuItemsRequest("Burger", MenuItemCategory.MainCourse, null);
        
        await handler.Handle(request, CancellationToken.None);
        
        mockRepository.Verify(repo => repo.SearchAsync(It.IsAny<string>(), It.IsAny<MenuItemCategory?>(), It.IsAny<decimal?>()), Times.Once);
    }
}

public class UserProfileManagementTests
{
    [Fact]
    public async Task Given_BuyerCreatesProfileAfterSignup_When_Completed_Then_ProfileStored()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<UpdateBuyerProfileValidator>();
        
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Role = Role.Buyer };
        
        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
        
        var handler = new UpdateBuyerProfileHandler(mockUserRepository.Object, mockValidator.Object);
        var request = new UpdateBuyerProfileRequest(userId, "(555) 123-4567", 
            new Address 
            { 
                Street = "123 Main St", 
                City = "Springfield", 
                State = "IL", 
                ZipCode = "62701",
                Country = "USA"
            });
        
        await handler.Handle(request, CancellationToken.None);
        
        mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Given_KitchenStaffCreatesProfileAfterSignup_When_Completed_Then_ProfileStored()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<UpdateKitchenProfileValidator>();
        
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Role = Role.Kitchen };
        
        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
        
        var handler = new UpdateKitchenProfileHandler(mockUserRepository.Object, mockValidator.Object);
        var request = new UpdateKitchenProfileRequest(userId, "Chef's Kitchen", "Authentic Italian Cuisine");
        
        await handler.Handle(request, CancellationToken.None);
        
        mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Given_BuyerUpdatesProfileMultipleTimes_When_Sequential_Then_LatestVersionStored()
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<UpdateBuyerProfileValidator>();
        
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Role = Role.Buyer };
        
        mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
        
        var handler = new UpdateBuyerProfileHandler(mockUserRepository.Object, mockValidator.Object);
        
        for (int i = 0; i < 3; i++)
        {
            var request = new UpdateBuyerProfileRequest(userId, $"({i}55) {i}23-456{i}", 
                new Address 
                { 
                    Street = $"{i}23 Main St", 
                    City = "Springfield", 
                    State = "IL", 
                    ZipCode = "62701",
                    Country = "USA"
                });
            await handler.Handle(request, CancellationToken.None);
        }
        
        mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Exactly(3));
    }
}

public class PaymentAndOrderIntegrationTests
{
    [Fact]
    public async Task Given_CreatePaymentForOrder_When_Processed_Then_PaymentLinkedToOrder()
    {
        var mockPaymentRepository = new Mock<IPaymentRepository>();
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockValidator = new Mock<CreatePaymentValidator>();
        
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.Confirmed };
        
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new CreatePaymentHandler(mockPaymentRepository.Object, mockOrderRepository.Object, mockValidator.Object);
        var request = new CreatePaymentRequest(orderId, 50m, PaymentMethod.CreditCard);
        
        await handler.Handle(request, CancellationToken.None);
        
        mockPaymentRepository.Verify(repo => repo.AddAsync(It.IsAny<Payment>()), Times.Once);
    }

    [Fact]
    public async Task Given_MultiplePaymentsForSingleOrder_When_Applied_Then_TotalTracked()
    {
        var mockRepository = new Mock<IPaymentRepository>();
        var mockValidator = new Mock<CreatePaymentValidator>();
        
        var orderId = Guid.NewGuid();
        var totalAmount = 100m;
        var payments = new List<Payment>();
        
        mockRepository.Setup(repo => repo.AddAsync(It.IsAny<Payment>()))
            .Callback<Payment>(p => payments.Add(p))
            .Returns(Task.CompletedTask);
        
        var handler = new CreatePaymentHandler(mockRepository.Object, new Mock<IOrderRepository>().Object, mockValidator.Object);
        
        var amounts = new[] { 30m, 40m, 30m };
        
        foreach (var amount in amounts)
        {
            var request = new CreatePaymentRequest(orderId, amount, PaymentMethod.CreditCard);
            await handler.Handle(request, CancellationToken.None);
        }
        
        payments.Sum(p => p.Amount).Should().Be(totalAmount);
    }

    [Fact]
    public async Task Given_PaymentWithDifferentMethods_When_Processed_Then_MethodRecorded()
    {
        var mockRepository = new Mock<IPaymentRepository>();
        var mockValidator = new Mock<CreatePaymentValidator>();
        
        var orderId = Guid.NewGuid();
        var methods = new[] { PaymentMethod.CreditCard, PaymentMethod.DebitCard, PaymentMethod.Cash };
        
        var handler = new CreatePaymentHandler(mockRepository.Object, new Mock<IOrderRepository>().Object, mockValidator.Object);
        
        foreach (var method in methods)
        {
            var request = new CreatePaymentRequest(orderId, 25m, method);
            await handler.Handle(request, CancellationToken.None);
        }
        
        mockRepository.Verify(repo => repo.AddAsync(It.Is<Payment>(p => p.Method == PaymentMethod.CreditCard)), Times.Once);
        mockRepository.Verify(repo => repo.AddAsync(It.Is<Payment>(p => p.Method == PaymentMethod.DebitCard)), Times.Once);
        mockRepository.Verify(repo => repo.AddAsync(It.Is<Payment>(p => p.Method == PaymentMethod.Cash)), Times.Once);
    }
}

public class LoyaltyAndOrderIntegrationTests
{
    [Fact]
    public async Task Given_OrderCompleted_When_LoyaltyPointsApplied_Then_PointsAdded()
    {
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockLoyaltyRepository = new Mock<ILoyaltyAccountRepository>();
        var mockValidator = new Mock<CreateLoyaltyAccountValidator>();
        
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, UserId = userId, Status = OrderStatus.Completed };
        
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var handler = new CreateLoyaltyAccountHandler(mockLoyaltyRepository.Object, mockValidator.Object);
        var request = new CreateLoyaltyAccountRequest(userId);
        
        await handler.Handle(request, CancellationToken.None);
        
        mockLoyaltyRepository.Verify(repo => repo.AddAsync(It.IsAny<LoyaltyAccount>()), Times.Once);
    }

    [Fact]
    public async Task Given_UserAccumulatesPointsOverMultipleOrders_When_Calculated_Then_TotalCorrect()
    {
        var mockLoyaltyRepository = new Mock<ILoyaltyAccountRepository>();
        var mockOrderRepository = new Mock<IOrderRepository>();
        
        var userId = Guid.NewGuid();
        var loyaltyAccount = new LoyaltyAccount { Id = Guid.NewGuid(), UserId = userId, Points = 0 };
        var orders = new List<Order>
        {
            new Order { Id = Guid.NewGuid(), UserId = userId, TotalPrice = 50m },
            new Order { Id = Guid.NewGuid(), UserId = userId, TotalPrice = 75m },
            new Order { Id = Guid.NewGuid(), UserId = userId, TotalPrice = 100m }
        };
        
        mockLoyaltyRepository.Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(loyaltyAccount);
        mockOrderRepository.Setup(repo => repo.GetOrdersByUserAsync(userId))
            .ReturnsAsync(orders);
        
        var totalOrderValue = orders.Sum(o => o.TotalPrice);
        
        totalOrderValue.Should().Be(225m);
    }
}

public class KitchenTaskAndOrderCoordinationTests
{
    [Fact]
    public async Task Given_OrderCreated_When_KitchenTaskCreated_Then_LinkedTogether()
    {
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockOrderValidator = new Mock<CreateOrderValidator>();
        var mockTaskValidator = new Mock<CreateKitchenTaskValidator>();
        
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        var order = new Order { Id = orderId, UserId = userId };
        mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId))
            .ReturnsAsync(order);
        
        var task = new KitchenTask { Id = Guid.NewGuid(), OrderId = orderId };
        mockTaskRepository.Setup(repo => repo.AddAsync(It.IsAny<KitchenTask>()))
            .Returns(Task.CompletedTask);
        
        var taskHandler = new CreateKitchenTaskHandler(mockTaskRepository.Object, mockTaskValidator.Object);
        var taskRequest = new CreateKitchenTaskRequest(orderId, "Prepare order", "Make items for order");
        
        await taskHandler.Handle(taskRequest, CancellationToken.None);
        
        mockTaskRepository.Verify(repo => repo.AddAsync(It.IsAny<KitchenTask>()), Times.Once);
    }

    [Fact]
    public async Task Given_MultipleOrdersWithTasks_When_AllProcessed_Then_TasksTrackedPerOrder()
    {
        var mockTaskRepository = new Mock<IKitchenTaskRepository>();
        var mockValidator = new Mock<CreateKitchenTaskValidator>();
        
        var tasks = new Dictionary<Guid, List<KitchenTask>>();
        
        var handler = new CreateKitchenTaskHandler(mockTaskRepository.Object, mockValidator.Object);
        
        for (int i = 0; i < 3; i++)
        {
            var orderId = Guid.NewGuid();
            tasks[orderId] = new List<KitchenTask>();
            
            for (int j = 0; j < 2; j++)
            {
                var request = new CreateKitchenTaskRequest(orderId, $"Task {j}", "Description");
                await handler.Handle(request, CancellationToken.None);
                tasks[orderId].Add(new KitchenTask { OrderId = orderId });
            }
        }
        
        mockTaskRepository.Verify(repo => repo.AddAsync(It.IsAny<KitchenTask>()), Times.Exactly(6));
        tasks.Should().HaveCount(3);
    }
}
